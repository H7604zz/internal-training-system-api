using Microsoft.AspNetCore.Identity;

namespace InternalTrainingSystem.Core.Helper
{
    public class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() => new IdentityError { Code = nameof(DefaultError), Description = "Đã xảy ra lỗi không xác định." };

        public override IdentityError ConcurrencyFailure() => new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Lỗi đồng thời, đối tượng đã bị sửa đổi." };

        public override IdentityError PasswordMismatch() => new IdentityError { Code = nameof(PasswordMismatch), Description = "Mật khẩu không đúng." };

        public override IdentityError InvalidToken() => new IdentityError { Code = nameof(InvalidToken), Description = "Token không hợp lệ." };

        public override IdentityError LoginAlreadyAssociated() => new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "Tài khoản này đã được liên kết với người dùng khác." };

        public override IdentityError InvalidUserName(string? userName) => new IdentityError { Code = nameof(InvalidUserName), Description = $"Tên người dùng '{userName}' không hợp lệ, chỉ có thể chứa chữ cái hoặc số." };

        public override IdentityError InvalidEmail(string? email) => new IdentityError { Code = nameof(InvalidEmail), Description = $"Email '{email}' không hợp lệ." };

        public override IdentityError DuplicateUserName(string userName) => new IdentityError { Code = nameof(DuplicateUserName), Description = $"Tên người dùng '{userName}' đã được sử dụng." };

        public override IdentityError DuplicateEmail(string email) => new IdentityError { Code = nameof(DuplicateEmail), Description = $"Email '{email}' đã được sử dụng." };

        public override IdentityError InvalidRoleName(string? role) => new IdentityError { Code = nameof(InvalidRoleName), Description = $"Tên vai trò '{role}' không hợp lệ." };

        public override IdentityError DuplicateRoleName(string role) => new IdentityError { Code = nameof(DuplicateRoleName), Description = $"Vai trò '{role}' đã tồn tại." };

        public override IdentityError UserAlreadyHasPassword() => new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "Người dùng đã có mật khẩu." };

        public override IdentityError UserLockoutNotEnabled() => new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "Khóa tài khoản không được bật cho người dùng này." };

        public override IdentityError UserAlreadyInRole(string role) => new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"Người dùng đã có vai trò '{role}'." };

        public override IdentityError UserNotInRole(string role) => new IdentityError { Code = nameof(UserNotInRole), Description = $"Người dùng không có vai trò '{role}'." };

        public override IdentityError PasswordTooShort(int length) => new IdentityError { Code = nameof(PasswordTooShort), Description = $"Mật khẩu phải có ít nhất {length} ký tự." };

        public override IdentityError PasswordRequiresNonAlphanumeric() => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Mật khẩu phải có ít nhất một ký tự đặc biệt." };

        public override IdentityError PasswordRequiresDigit() => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Mật khẩu phải có ít nhất một chữ số ('0'-'9')." };

        public override IdentityError PasswordRequiresLower() => new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Mật khẩu phải có ít nhất một chữ cái thường ('a'-'z')." };

        public override IdentityError PasswordRequiresUpper() => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Mật khẩu phải có ít nhất một chữ cái viết hoa ('A'-'Z')." };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new IdentityError { Code = nameof(PasswordRequiresUniqueChars), Description = $"Mật khẩu phải có ít nhất {uniqueChars} ký tự khác nhau." };

        public override IdentityError RecoveryCodeRedemptionFailed() => new IdentityError { Code = nameof(RecoveryCodeRedemptionFailed), Description = "Sử dụng mã khôi phục thất bại." };
    }
}
