#!/bin/bash

set -e

echo "MgTx Service 卸载脚本 - 猛攻通讯"
echo "================================"

if [ "$EUID" -ne 0 ]; then 
   echo "请使用 sudo 运行此脚本"
   exit 1
fi

SERVICE_NAME="mgtx"
INSTALL_DIR="/opt/mgtx"
LOG_DIR="/var/log/mgtx"

# Stop service
echo "停止服务..."
systemctl stop $SERVICE_NAME 2>/dev/null || true
systemctl disable $SERVICE_NAME 2>/dev/null || true

# Remove systemd service file
echo "移除 systemd 服务文件..."
rm -f /etc/systemd/system/$SERVICE_NAME.service
systemctl daemon-reload

# Remove installation directory
echo "移除安装目录..."
rm -rf $INSTALL_DIR

# Optionally remove logs
read -p "是否删除日志文件? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    rm -rf $LOG_DIR
    echo "日志文件已删除"
fi

echo ""
echo "✓ 卸载完成"
