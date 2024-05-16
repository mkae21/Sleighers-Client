using System.Collections.Generic;

namespace API
{
    public enum Type
    {
        login,
        loginSucc,
        signup,
        setName,
        matching,
    }
}

// 구글 로그인 - 이메일
// 그냥 로그인 - 이메일, 비번
// 회원가입 - 이메일, 비번, 닉네임
// 매치메이킹 - 닉네임
public class PlayerInfo
{
    public string email = string.Empty;
    public string password = string.Empty;
    public string nickname = string.Empty;
    public override string ToString()
    {
        return string.Format("email: {0}, password: {1}, nickname: {2}", email, password, nickname);
    }
}

public class RoomData
{
    public int roomID;
    public List<PlayerInfo> playerList;
}