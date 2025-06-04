using System;

namespace Server
{
    public class AuthService
    {
        private readonly UserRepository userRepository;

        public AuthService(UserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public string Register(string username, string email, string password)
        {
            if (userRepository.UserExists(email))
                return "error|Пользователь с таким email уже существует.|";

            var user = new User
            {
                UserName = username,
                Email = email,
                Password = password
            };

            userRepository.AddUser(user);
            return "success|Регистрация успешна.|";
        }

        public string Authenticate(string email, string password)
        {
            return userRepository.ValidateCredentials(email, password)
                ? "success|Вход выполнен успешно.|"
                : "error|Неверный email или пароль.|";
        }
    }
}