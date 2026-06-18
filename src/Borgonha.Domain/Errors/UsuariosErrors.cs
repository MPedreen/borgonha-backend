namespace Borgonha.Domain.Errors;

public static class UsuariosErrors
{
    public static class Usuario
    {
        public static readonly Error JaExiste =
            Error.Conflito("Usuario.JaExiste", "Já existe um usuário com este username ou e-mail.");

        public static readonly Error RoleInvalida =
            Error.Validacao("Usuario.RoleInvalida", "O perfil deve ser 'admin' ou 'atendente'.");

        public static readonly Error ErroCriar =
            Error.Inesperado("Usuario.ErroCriar", "Não foi possível criar o usuário no servidor de autenticação.");
    }
}
