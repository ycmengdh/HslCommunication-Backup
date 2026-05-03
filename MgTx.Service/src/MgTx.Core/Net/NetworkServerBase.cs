using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace MgTx.Core.Net;

public class NetworkServerBase : IDisposable
{
    protected readonly ILogger? Logger;
    protected Socket? ServerSocket;
    protected readonly Dictionary<string, Socket> Clients = new();
    protected CancellationTokenSource? ServerCts;
    protected bool IsRunning;
    
    public int Port { get; private set; }
    public int MaxConnections { get; set; } = 100;
    public int ReceiveBufferSize { get; set; } = 4096;

    protected NetworkServerBase(ILogger? logger = null)
    {
        Logger = logger;
    }

    public async Task<OperateResult> StartAsync(int port, CancellationToken cancellationToken = default)
    {
        try
        {
            Port = port;
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            ServerSocket.Listen(MaxConnections);
            
            IsRunning = true;
            ServerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            Logger?.LogInformation("TCP 服务器已启动，监听端口 {Port}", port);
            
            _ = AcceptClientsAsync(ServerCts.Token);
            
            return OperateResult.Success();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "启动 TCP 服务器失败");
            return OperateResult.Fail($"启动失败: {ex.Message}", 1008);
        }
    }

    private async Task AcceptClientsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && IsRunning)
        {
            try
            {
                var client = await ServerSocket!.AcceptAsync(cancellationToken);
                var clientId = Guid.NewGuid().ToString("N");
                
                Clients[clientId] = client;
                Logger?.LogInformation("客户端已连接: {ClientId} from {EndPoint}", clientId, client.RemoteEndPoint);
                
                OnClientConnected(clientId, client);
                _ = HandleClientAsync(clientId, client, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "接受客户端连接时发生错误");
            }
        }
    }

    private async Task HandleClientAsync(string clientId, Socket client, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[ReceiveBufferSize];
        
        try
        {
            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                int received = await client.ReceiveAsync(buffer.AsMemory(), SocketFlags.None, cancellationToken);
                if (received == 0)
                {
                    break;
                }
                
                OnDataReceived(clientId, buffer[..received]);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "处理客户端数据时发生错误: {ClientId}", clientId);
        }
        finally
        {
            Clients.Remove(clientId);
            OnClientDisconnected(clientId);
            try { client.Shutdown(SocketShutdown.Both); } catch { }
            client.Dispose();
            Logger?.LogInformation("客户端已断开: {ClientId}", clientId);
        }
    }

    protected virtual void OnClientConnected(string clientId, Socket client) { }
    protected virtual void OnClientDisconnected(string clientId) { }
    protected virtual void OnDataReceived(string clientId, byte[] data) { }

    public async Task<OperateResult> SendToClientAsync(string clientId, byte[] data, CancellationToken cancellationToken = default)
    {
        if (!Clients.TryGetValue(clientId, out var client))
        {
            return OperateResult.Fail("客户端不存在", 1003);
        }

        try
        {
            await client.SendAsync(data.AsMemory(), SocketFlags.None, cancellationToken);
            return OperateResult.Success();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "发送数据到客户端失败: {ClientId}", clientId);
            return OperateResult.Fail($"发送失败: {ex.Message}", 1005);
        }
    }

    public void Stop()
    {
        try
        {
            IsRunning = false;
            ServerCts?.Cancel();
            
            foreach (var client in Clients.Values)
            {
                try { client.Shutdown(SocketShutdown.Both); } catch { }
                client.Dispose();
            }
            Clients.Clear();
            
            ServerSocket?.Close();
            ServerSocket?.Dispose();
            ServerSocket = null;
            
            Logger?.LogInformation("TCP 服务器已停止");
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "停止服务器时发生错误");
        }
    }

    public void Dispose()
    {
        Stop();
        ServerCts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
