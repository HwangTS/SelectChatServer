#include <algorithm>

#include "../ServerNetLib/ILog.h"
#include "../ServerNetLib/TcpNetwork.h"
#include "../../Common/Packet.h"
#include "../../Common/ErrorCode.h"

#include "User.h"
#include "Game.h"
#include "Room.h"

using PACKET_ID = NCommon::PACKET_ID;

namespace NLogicLib
{
	Room::Room() {}

	Room::~Room()
	{
		if (m_pGame != nullptr) {
			delete m_pGame;
		}
	}
	
	void Room::Init(const short index, const short maxUserCount)
	{
		m_Index = index;
		m_MaxUserCount = maxUserCount;

		m_pGame = new Game;
	}

	void Room::SetNetwork(TcpNet* pNetwork, ILog* pLogger)
	{
		m_pRefLogger = pLogger;
		m_pRefNetwork = pNetwork;
	}

	void Room::Clear()
	{
		m_IsUsed = false;
		m_Title = L"";
		m_UserList.clear();
	}

	Game* Room::GetGameObj()
	{
		return m_pGame;
	}

	ERROR_CODE Room::CreateRoom(const wchar_t* pRoomTitle)
	{
		if(this == NULL)
			return ERROR_CODE::ROOM_ENTER_CREATE_FAIL;

		if (m_IsUsed) {
			return ERROR_CODE::ROOM_ENTER_CREATE_FAIL;
		}

		m_IsUsed = true;
		m_Title = pRoomTitle;

		return ERROR_CODE::NONE;
	}

	ERROR_CODE Room::EnterUser(User* pUser)
	{
		if (m_IsUsed == false) {
			return ERROR_CODE::ROOM_ENTER_NOT_CREATED;
		}

		if (m_UserList.size() == m_MaxUserCount) {
			return ERROR_CODE::ROOM_ENTER_MEMBER_FULL;
		}

		m_UserList.push_back(pUser);
		return ERROR_CODE::NONE;
	}

	bool Room::IsMaster(const short userIndex)
	{
		return m_UserList[0]->GetIndex() == userIndex ? true : false;
	}

	ERROR_CODE Room::LeaveUser(const short userIndex)
	{
		if (m_IsUsed == false) {
			return ERROR_CODE::ROOM_ENTER_NOT_CREATED;
		}

		auto iter = std::find_if(std::begin(m_UserList), std::end(m_UserList), [userIndex](auto pUser) { return pUser->GetIndex() == userIndex; });
		if (iter == std::end(m_UserList)) {
			return ERROR_CODE::ROOM_LEAVE_NOT_MEMBER;
		}
		
		m_UserList.erase(iter);

		if (m_UserList.empty()) 
		{
			Clear();
		}

		return ERROR_CODE::NONE;
	}

	void Room::SendToAllUser(const short packetId, const short dataSize, char* pData, const int passUserindex)
	{
		for (auto pUser : m_UserList)
		{
			// 자기 자신한테도 보낸다.
			//if (pUser->GetIndex() == passUserindex) {
			//	continue;
			//}

			m_pRefNetwork->SendData(pUser->GetSessioIndex(), packetId, dataSize, pData);
		}
	}

	void Room::NotifyEnterUserInfo(const int userIndex, const char* pszUserID)
	{
		NCommon::PktRoomEnterUserInfoNtf pkt;
		pkt.UserUniqueId = userIndex;
		//pkt.length = NCommon::MAX_USER_ID_SIZE;
		strncpy_s(pkt.UserID, _countof(pkt.UserID), pszUserID, NCommon::MAX_USER_ID_SIZE);

		SendToAllUser((short)PACKET_ID::ROOM_ENTER_NEW_USER_NTF, sizeof(pkt), (char*)&pkt, userIndex);
	}

	// TODO : 190416
	// 기존 유저 알람 패킷 작업해야할 차례. 이것이 해결이 되어야 채팅을 할 수 있다. 유저정보를 유지해야하므로..
	// room안의 유저를 알려줌.
	void Room::NotifyRoomUserInfo(const int sessionIndex, const int userCount, const std::vector<User*>& userList)
	{
		NCommon::PktRoomUserInfoNtf pkt;		

		pkt.UserCount = userCount;
		
		for (int i = 0; i < userCount; i++)
		{
			pkt.roomUserInfo[i].UserUniqueId = userList[i]->GetIndex();
			strncpy_s(pkt.roomUserInfo[i].UserID, _countof(pkt.roomUserInfo[i].UserID), userList[i]->GetID().c_str(), NCommon::MAX_USER_ID_SIZE);
		}

		m_pRefNetwork->SendData(sessionIndex, (short)PACKET_ID::ROOM_USER_LIST_NTF, sizeof(pkt), (char*)& pkt);
	}

	void Room::NotifyLeaveUserInfo(const char* pszUserID)
	{
		if (m_IsUsed == false) {
			return;
		}

		NCommon::PktRoomLeaveUserInfoNtf pkt;
		strncpy_s(pkt.UserID, _countof(pkt.UserID), pszUserID, NCommon::MAX_USER_ID_SIZE);

		SendToAllUser((short)PACKET_ID::ROOM_LEAVE_USER_NTF, sizeof(pkt), (char*)&pkt);
	}

	//void Room::NotifyChat(const int sessionIndex, const char* pszUserID, const wchar_t* pszMsg)
	void Room::NotifyChat(const int sessionIndex, const int userUniqueId, const wchar_t* pszMsg)
	{
		NCommon::PktRoomChatNtf pkt;
		//strncpy_s(pkt.UserID, _countof(pkt.UserID), pszUserID, NCommon::MAX_USER_ID_SIZE);
		pkt.UserUniqueId = userUniqueId;
		pkt.msgLen = static_cast<int>(wcslen(pszMsg));
		wcsncpy_s(pkt.Msg, NCommon::MAX_ROOM_CHAT_MSG_SIZE + 1, pszMsg, NCommon::MAX_ROOM_CHAT_MSG_SIZE);

		SendToAllUser((short)PACKET_ID::ROOM_CHAT_NTF, sizeof(pkt), (char*)&pkt, sessionIndex);
	}

	void Room::Update()
	{
		if (m_pGame->GetState() == GameState::ING)
		{
			if (m_pGame->CheckSelectTime())
			{
				//선택 안하는 사람이 지도록 한
			}
		}
	}
}