using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_test_client
{
    struct PacketData
    {
        public Int16 DataSize;
        public Int16 PacketID;
        public SByte Type;
        public byte[] BodyData;
    }

    public class PacketDump
    {
        public static string Bytes(byte[] byteArr)
        {
            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < byteArr.Length; ++i)
            {
                sb.Append(byteArr[i] + " ");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
    

    public class ErrorNtfPacket
    {
        public ERROR_CODE Error;

        public bool FromBytes(byte[] bodyData)
        {
            Error = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }
    

    public class LoginReqPacket
    {
        byte[] UserID = new byte[PacketDef.MAX_USER_ID_BYTE_LENGTH];
        byte[] UserPW = new byte[PacketDef.MAX_USER_PW_BYTE_LENGTH];

        public void SetValue(string userID, string userPW)
        {
            Encoding.UTF8.GetBytes(userID).CopyTo(UserID, 0);
            Encoding.UTF8.GetBytes(userPW).CopyTo(UserPW, 0);
        }

        public byte[] ToBytes()
        {
            List<byte> dataSource = new List<byte>();
            dataSource.AddRange(UserID);
            dataSource.AddRange(UserPW);
            return dataSource.ToArray();
        }
    }

    public class LoginResPacket
    {
        public Int16 Result;

        public bool FromBytes(byte[] bodyData)
        {
            Result = BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }
    



    // LobbyList
    public class LobbyListReqPacket
    {
    }

    public class LobbyListResPacket
    {
        public Int16 Result;
        public short LobbyCount;

        public LobbyListInfo[] lobbyListinfo = new LobbyListInfo[PacketDef.MAX_LOBBY_LIST_COUNT_BYTE_LENGTH];

        public bool FromBytes(byte[] bodyData)
        {
            //Result = BitConverter.ToInt16(bodyData, 0);
            //LobbyCount = BitConverter.ToInt16(bodyData, 2);
            var readPos = 0;
            Result = (SByte)bodyData[readPos];
            readPos += 2;

            LobbyCount = (SByte)bodyData[readPos];
            readPos += 2;

            for(int i = 0; i < LobbyCount; i++)
            {
                var lobbyId = (SByte)bodyData[readPos];
                readPos += 2;
                
                var lobbyUserCount = (SByte)bodyData[readPos];
                readPos += 2;

                var lobbyMaxUserCount = (SByte)bodyData[readPos];
                readPos += 2;

                LobbyListInfo temp = new LobbyListInfo();
                temp.SetValue(lobbyId, lobbyUserCount, lobbyMaxUserCount);

                lobbyListinfo[i] = temp;
            }

            return true;
        }
    }

    //LobbyEnter
    public class LobbyEnterReqPacket
    {
        int LobbyId;

        public void SetValue(int lobbyId)
        {
            LobbyId = lobbyId;
        }

        public byte[] ToBytes()
        {
            List<byte> dataSource = new List<byte>();
            dataSource.AddRange(BitConverter.GetBytes(LobbyId));
            return dataSource.ToArray();
        }
    }

    public class LobbyEnterResPacket
    {
        public Int16 Result;
        public Int16 MaxUserCount;
        public Int16 MaxRoomCount;

        public bool FromBytes(byte[] bodyData)
        {
            Result = BitConverter.ToInt16(bodyData, 0);
            MaxUserCount = BitConverter.ToInt16(bodyData, 2);
            MaxRoomCount = BitConverter.ToInt16(bodyData, 4);

            return true;
        }
    }


    public class RoomEnterReqPacket
    {
        bool IsCreate;
        short RoomNumber;

        byte[] RoomTitle = new byte[PacketDef.MAX_ROOM_TITLE_SIZE];

        public void SetValue(bool isCreate, short roomNumber, string roomTitle)
        {
            IsCreate = isCreate;
            RoomNumber = roomNumber;
            Encoding.UTF8.GetBytes(roomTitle).CopyTo(RoomTitle, 0);
        }

        public byte[] ToBytes()
        {
            List<byte> dataSource = new List<byte>();
            dataSource.AddRange(BitConverter.GetBytes(IsCreate));
            dataSource.AddRange(BitConverter.GetBytes(RoomNumber));
            dataSource.AddRange(RoomTitle);
            return dataSource.ToArray();
        }
    }

    public class RoomEnterResPacket
    {
        public Int16 Result;
        public Int32 RoomUserUniqueId;

        public bool FromBytes(byte[] bodyData)
        {
            Result = BitConverter.ToInt16(bodyData, 0);
            RoomUserUniqueId = BitConverter.ToInt32(bodyData, 4);

            return true;
        }
    }

    public class RoomUserListNtfPacket
    {
        public int UserCount = 0;
        // public List<Int64> UserUniqueIdList = new List<Int64>();
        public List<Int32> UserUniqueIdList = new List<Int32>();
        public List<string> UserIDList = new List<string>();

        public bool FromBytes(byte[] bodyData)
        {
            var readPos = 0;
            var userCount = BitConverter.ToInt32(bodyData, 0);
            readPos += 4;

            for (int i = 0; i < userCount; ++i)
            {
                var uniqeudId = BitConverter.ToInt16(bodyData, readPos);
                readPos += 4;

                var id = Encoding.UTF8.GetString(bodyData, readPos, PacketDef.MAX_USER_ID_BYTE_LENGTH + 1);
                readPos += PacketDef.MAX_USER_ID_BYTE_LENGTH + 1;

                UserUniqueIdList.Add(uniqeudId);
                UserIDList.Add(id);
            }

            UserCount = userCount;
            return true;
        }
    }

    public class RoomNewUserNtfPacket
    {
        public Int32 UserUniqueId;
        public string UserID;

        public bool FromBytes(byte[] bodyData)
        {
            var readPos = 0;

            //UserUniqueId = BitConverter.ToInt64(bodyData, readPos);
            //readPos += 8;

            UserUniqueId = BitConverter.ToInt32(bodyData, readPos);
            readPos += 4;

            //var idlen = (SByte)bodyData[readPos];
            //++readPos;

            UserID = Encoding.UTF8.GetString(bodyData, readPos, PacketDef.MAX_USER_ID_BYTE_LENGTH + 1);
            readPos += PacketDef.MAX_USER_ID_BYTE_LENGTH + 1;

            return true;
        }
    }


    public class RoomChatReqPacket
    {
        Int16 MsgLen;
        byte[] Msg;//= new byte[PacketDef.MAX_USER_ID_BYTE_LENGTH];

        public void SetValue(string message)
        {
            Msg = Encoding.UTF8.GetBytes(message);
            MsgLen = (Int16)Msg.Length;
        }

        public byte[] ToBytes()
        {
            List<byte> dataSource = new List<byte>();
            dataSource.AddRange(BitConverter.GetBytes(MsgLen));
            dataSource.AddRange(Msg);
            return dataSource.ToArray();
        }
    }

    public class RoomChatResPacket
    {
        public Int16 Result;
        
        public bool FromBytes(byte[] bodyData)
        {
            Result = BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }

    public class RoomChatNtfPacket
    {
        //public Int64 UserUniqueId;
        public Int32 UserUniqueId;
        public string Message;

        public bool FromBytes(byte[] bodyData)
        {
            //UserUniqueId = BitConverter.ToInt64(bodyData, 0);
            UserUniqueId = BitConverter.ToInt32(bodyData, 0);

            //var msgLen = BitConverter.ToInt16(bodyData, 8);
            var msgLen = BitConverter.ToInt16(bodyData, 4);
            byte[] messageTemp = new byte[msgLen];
            Buffer.BlockCopy(bodyData, 8 + 2, messageTemp, 0, msgLen);
            Message = Encoding.UTF8.GetString(messageTemp);
            return true;
        }
    }


     public class RoomLeaveResPacket
    {
        public Int16 Result;
        
        public bool FromBytes(byte[] bodyData)
        {
            Result = BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }

    public class RoomLeaveUserNtfPacket
    {
        public Int64 UserUniqueId;

        public bool FromBytes(byte[] bodyData)
        {
            UserUniqueId = BitConverter.ToInt64(bodyData, 0);
            return true;
        }
    }


    
    public class RoomRelayNtfPacket
    {
        public Int64 UserUniqueId;
        public byte[] RelayData;

        public bool FromBytes(byte[] bodyData)
        {
            UserUniqueId = BitConverter.ToInt64(bodyData, 0);

            var relayDataLen = bodyData.Length - 8;
            RelayData = new byte[relayDataLen];
            Buffer.BlockCopy(bodyData, 8, RelayData, 0, relayDataLen);
            return true;
        }
    }


    public class PingRequest
    {
        public Int16 PingNum;

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(PingNum);
        }

    }
}
