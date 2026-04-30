namespace SalamHack.Domain.Common.Results;

public readonly record struct Error
{
    private Error(string code, string description, ErrorKind type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }

    public string Description { get; }

    public ErrorKind Type { get; }

    public static Error Failure(string code = nameof(Failure), string description = "حدث خطأ عام.")
        => new(code, description, ErrorKind.Failure);

    public static Error Unexpected(string code = nameof(Unexpected), string description = "حدث خطأ غير متوقع.")
        => new(code, description, ErrorKind.Unexpected);

    public static Error Validation(string code = nameof(Validation), string description = "خطأ في التحقق من البيانات.")
        => new(code, description, ErrorKind.Validation);

    public static Error Conflict(string code = nameof(Conflict), string description = "يوجد تعارض في البيانات.")
        => new(code, description, ErrorKind.Conflict);

    public static Error NotFound(string code = nameof(NotFound), string description = "لم يتم العثور على العنصر المطلوب.")
        => new(code, description, ErrorKind.NotFound);

    public static Error Unauthorized(string code = nameof(Unauthorized), string description = "غير مصرح بهذه العملية.")
        => new(code, description, ErrorKind.Unauthorized);

    public static Error Forbidden(string code = nameof(Forbidden), string description = "ليس لديك صلاحية للقيام بهذه العملية.")
        => new(code, description, ErrorKind.Forbidden);

    public static Error Create(int type, string code, string description)
        => new(code, description, (ErrorKind)type);
}
