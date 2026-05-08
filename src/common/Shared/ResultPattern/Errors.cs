namespace Shared.Results;

public static class Errors
{
    public static class General
    {
        public static Error ValueIsInvalid(string? name = null)
        {
            var label = name ?? "value";
            return Error.Validation("value.is.invalid", $"{label} недопустимое значение");
        }

        public static Error ValueIsInvalidLength(string? name = null)
        {
            var label = name ?? "value";
            
            return Error.Validation($"length.is.invalid", $"{label} не правильная длина");
        }

        public static Error TokenExpired(string? name = null)
        {
            return Error.Iternal("token.is.expired","Token expired");
        } 

        public static Error ValueIsRequired(string? name = null)
        {
            var label = name ?? "value";
            return Error.Validation("value.is.required", $"{label} отсутствует");
        }

        public static Error AlreadyExist()
        {
            return Error.Validation("record.already.exist", "Запись уже существует");
        }

        public static Error Iternal()
        {
            return Error.Iternal("server.failure", "Server failure");
        }

        public static Error NotAllowed()
        {
            return Error.Iternal("not.allowed", "Not allowed");
        }
    }

    public static class UserErrors
    {
        public static Error MissingId()
        {
            return Error.Validation("id.missing", "Id пользователя отсутсвует");
        }

        public static Error EmailNotVerified()
        {
            return Error.Validation("email.not_verified", "Подтвердите свой email");
        }

        public static Error EmailDuplicate(string? email = null)
        {
            var forEmail = email == null ? "" : $"{email}";
            return Error.Conflict("email.already.used", $"этот {forEmail} уже занят");
        }

        public static Error NotFound(Guid? id = null, string? email = null)
        {
            var forId = id == null ? "" : $" по id'{id}'";
            var forEmal = email == null ? "" : $"по email'{email}'";
            return Error.NotFound("user.not.found", $"пользователь не найден {forId}{forEmal}");
        }

        public static Error IncorrectPassword()
        {
            return Error.Validation("password.incorrect", "Не правильный пароль");
        }
    }

    public static class TrackErrors
    {
        public static Error MissingId()
        {
            return Error.Validation("id.missing", "Id песни отсутствует");
        }

        public static Error NotFound(Guid? id = null)
        {
            var forId = id == null ? "" : $" для id: '{id}'";
            return Error.NotFound("track.not.found", $"track not found{forId}");
        }
    }

    public static class FileErrors
    {
        public static Error MissingKey()
        {
            return Error.Validation("key.missing", "Ключ файла отсутсвует");
        }

        public static Error MissingFilePath()
        {
            return Error.Validation("filePath.missing", "Путь до файла отсутсвует");
        }

        public static Error NotFound(Guid? id = null)
        {
            var forId = id == null ? "" : $" по Id '{id}'";
            return Error.NotFound("track.not.found", $"Песня не найдена {forId}");
        }

        public static Error UploadError()
        {
            return Error.Iternal("upload.faild", "Что то пошло не так при загрузке файла");
        }

        public static Error MissingFile(string? name = null)
        {
            return Error.Validation("file.null", $"{name} файл отсутсвует или нул");
        }

        public static Error InvalidType(string[] types)
        {
            return Error.Validation("file.invalid.type", $"Тип фала должен быть: {string.Join(", ", types)}");
        }

        public static Error InvalidSize(int size)
        {
            return Error.Validation("file.invalid.size", $"Размер файла должен быть меньше {size} mb");
        }
    }
}
