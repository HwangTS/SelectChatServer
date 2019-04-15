cmake_minimum_required(VERSION 3.13)
project(ServerNetLib)

set(CMAKE_CXX_STANDARD 14)

include_directories(.)

add_executable(ServerNetLib
        Define.h
        ILog.h
        ITcpNetwork.h
        ServerNetErrorCode.h
        TcpNetwork.cpp
        TcpNetwork.h)
