import { SincronizarItens } from "./sincronizar-itens";

export class Sincronizar {
  id?: number;
  dataInicio?: Date;
  dataFim?: Date;
  status?: number;
  itens?: SincronizarItens[];
}
