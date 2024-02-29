namespace Back.Dominio
{
    public static class Constantes
    {
        #region STATUS
        public const string STATUS_NOVO = "New";
        #endregion

        #region TIPO_ITEM
        public const string TIPO_ITEM_TASK = "Task";
        public const string TIPO_ITEM_BUG = "Bug";
        public const string TIPO_ITEM_SOLICITACAO = "Solicitação";
        public const string TIPO_ITEM_HISTORIA = "User Story";
        public const string TIPO_ITEM_ENABLER = "Enabler";
        #endregion

        #region NOTIFICACAO
        public const string NOTIFICACAO_GRUPO_LOCAL = "GrupoLocal";
        public const string NOTIFICACAO_INICIO = "SincronizacaoInicio";
        public const string NOTIFICACAO_PRONTA = "SincronizacaoFim";
        #endregion


        #region JOB
        public const string JOB_UM = "SincronizarJob";
        public const string JOB_UM_TRIGGER = "SincronizarJobTrigger";
        public const string JOB_GRUPO_UM = "New";
        #endregion
    }
}
