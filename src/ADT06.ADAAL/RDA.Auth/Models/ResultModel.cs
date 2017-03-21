public class AuthResultModel
{
    public enum AuthAction
    {
        Logon,
        Logoff
    }

    private AuthAction _action;

    public string Action { get { return _action.ToString(); } }
    public string UserName { get; set; }
    public bool Succeeded { get; set; }
    public string[] Reasons { get; set; }

    public AuthResultModel(AuthAction action) { _action = action; }
}