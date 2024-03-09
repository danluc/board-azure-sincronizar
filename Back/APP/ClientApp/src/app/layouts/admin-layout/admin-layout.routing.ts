import { Routes } from "@angular/router";

import { DashboardComponent } from "../../dashboard/dashboard.component";
import { ContasComponent } from "app/contas/contas.component";
import { ConfiguracoesComponent } from "app/configuracoes/configuracoes.component";
import { ListarContasComponent } from "app/contas/listar-contas/listar-contas.component";
import { CadastrarContaComponent } from "app/contas/cadastrar-conta/cadastrar-conta.component";
import { AzureComponent } from "app/azure/azure.component";
import { ListarAzureComponent } from "app/azure/listar-azure/listar-azure.component";
import { CadastrarAzureComponent } from "app/azure/cadastrar-azure/cadastrar-azure.component";

export const AdminLayoutRoutes: Routes = [
  { path: "dashboard", component: DashboardComponent },
  {
    path: "azure",
    component: AzureComponent,
    children: [
      { path: "", component: ListarAzureComponent },
      { path: "add", component: CadastrarAzureComponent },
    ],
  },
  {
    path: "contas",
    component: ContasComponent,
    children: [
      { path: "", component: ListarContasComponent },
      { path: "add", component: CadastrarContaComponent },
    ],
  },
  { path: "configuracoes", component: ConfiguracoesComponent },
];
