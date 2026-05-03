#!/bin/bash

set -e

echo "MgTx Service 安装脚本 - 猛攻通讯"
echo "================================"

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
   echo "请使用 sudo 运行此脚本"
   exit 1
fi

# Variables
SERVICE_NAME="mgtx"
SERVICE_USER="mgtx"
INSTALL_DIR="/opt/mgtx"
LOG_DIR="/var/log/mgtx"

# Create service user if not exists
if ! id -u $SERVICE_USER > /dev/null 2>&1; then
    echo "创建服务用户: $SERVICE_USER"
    useradd --system --no-create-home --shell=/usr/sbin/nologin $SERVICE_USER
fi

# Create directories
echo "创建目录..."
mkdir -p $INSTALL_DIR
mkdir -p $LOG_DIR
mkdir -p $INSTALL_DIR/logs

# Stop service if running
if systemctl is-active --quiet $SERVICE_NAME; then
    echo "停止现有服务..."
    systemctl stop $SERVICE_NAME
fi

# Copy files
echo "复制服务文件..."
cp -r ./publish/* $INSTALL_DIR/
chown -R $SERVICE_USER:$SERVICE_USER $INSTALL_DIR
chown -R $SERVICE_USER:$SERVICE_USER $LOG_DIR

# Set permissions
chmod +x $INSTALL_DIR/MgTx.Service
chmod 755 $INSTALL_DIR

# Copy systemd service file
echo "安装 systemd 服务..."
cp deployment/linux/mgtx.service /etc/systemd/system/
systemctl daemon-reload

# Enable and start service
echo "启用并启动服务..."
systemctl enable $SERVICE_NAME
systemctl start $SERVICE_NAME

# Check status
sleep 2
if systemctl is-active --quiet $SERVICE_NAME; then
    echo "✓ 服务已成功安装并启动"
    echo ""
    echo "服务状态:"
    systemctl status $SERVICE_NAME --no-pager
    echo ""
    echo "访问地址: http://localhost:8080/api/v1/health"
else
    echo "✗ 服务启动失败，请检查日志"
    journalctl -u $SERVICE_NAME --no-pager -n 20
    exit 1
fi
