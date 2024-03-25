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
        public const string TIPO_ITEM_DEBITO = "Débito Técnico";
        public const string TIPO_ITEM_STORY_ENABLER = "Story Enabler";
        public const string TIPO_ITEM_STORY = "Story";
        public const string TIPO_ITEM_INCIDENTE = "Incident";
        #endregion

        #region NOTIFICACAO
        public const string NOTIFICACAO_GRUPO_LOCAL = "GrupoLocal";
        public const string NOTIFICACAO_INICIO = "SistemaIniciado";
        public const string NOTIFICACAO_SYNC_INICIO = "SincronizacaoInicio";
        public const string NOTIFICACAO_SYNC_FIM = "SincronizacaoFim";
        #endregion


        #region JOB
        public const string JOB_UM = "SincronizarJob";
        public const string JOB_UM_TRIGGER = "SincronizarJobTrigger";
        public const string JOB_GRUPO_UM = "New";
        #endregion
    }
}
