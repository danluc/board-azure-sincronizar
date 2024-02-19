export class TeamProjectReference {
  id?: string;
  name?: string;
  path?: string;
}

export class BuscarProjetoDTO {
  url?: string;
  token?: string;
}

export class BuscarIterationsDTO {
  url?: string;
  token?: string;
  projetoNome?: string;
  timeNome?: string;
  areaNome?: string;
}
