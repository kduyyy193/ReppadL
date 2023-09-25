using ReppadL.Common;
using ReppadL.Model;
using ReppadL.Model.PostModel;
using ReppadL.Model.Response;

namespace ReppadL.IRepo
{
	public interface IAuthorizeRepo
    {
        Task<APIResponse<TokenResponse>> Register(PostUserRegister user);
        Task<APIResponse<TokenResponse>> Login(PostUserLogin user);
    }
}

