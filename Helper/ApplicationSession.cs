namespace StudentGovernanceStudentWeb.Helper;

public class ApplicationSession(IHttpContextAccessor httpContextAccessor) : IApplicationSession
{
    public string? Token
    {
        get => httpContextAccessor.HttpContext?.Session.GetString("Token");
        set
        {
            if (value != null) httpContextAccessor.HttpContext?.Session.SetString("Token", value);
        }
    }

    public string? StudentNumber
    {
        get => httpContextAccessor.HttpContext?.Session.GetString("StudentNumber");
        set
        {
            if (value != null) httpContextAccessor.HttpContext?.Session.SetString("StudentNumber", value);
        }
    }

    public string? StudentName
    {
        get => httpContextAccessor.HttpContext?.Session.GetString("StudentName");
        set
        {
            if (value != null) httpContextAccessor.HttpContext?.Session.SetString("StudentName", value);
        }
    }

    public string? StudentIdNumber
    {
        get => httpContextAccessor.HttpContext?.Session.GetString("StudentIdNumber");
        set
        {
            if (value != null) httpContextAccessor.HttpContext?.Session.SetString("StudentIdNumber", value);
        }
    }

    public string? Role
    {
        get => httpContextAccessor.HttpContext?.Session.GetString("Role");
        set
        {
            if (value != null) httpContextAccessor.HttpContext?.Session.SetString("Role", value);
        }
    }

    public string? Email
    {
        get => httpContextAccessor.HttpContext?.Session.GetString("Email");
        set
        {
            if (value != null) httpContextAccessor.HttpContext?.Session.SetString("Email", value);
        }
    }

    public string? FullName
    {
        get => httpContextAccessor.HttpContext?.Session.GetString("FullName");
        set
        {
            if (value != null) httpContextAccessor.HttpContext?.Session.SetString("FullName", value);
        }
    }

    public string? Username
    {
        get => httpContextAccessor.HttpContext?.Session.GetString("Username");
        set
        {
            if (value != null) httpContextAccessor.HttpContext?.Session.SetString("Username", value);
        }
    }

    public string? StaffNumber
    {
        get => httpContextAccessor.HttpContext?.Session.GetString("StaffNumber");
        set
        {
            if (value != null) httpContextAccessor.HttpContext?.Session.SetString("StaffNumber", value);
        }
    }

    public string? ApplicationType
    {
        get => httpContextAccessor.HttpContext?.Session.GetString("ApplicationType");
        set
        {
            if (value != null) httpContextAccessor.HttpContext?.Session.SetString("ApplicationType", value);
        }
    }
}