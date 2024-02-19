import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";
import { RouterModule } from "@angular/router";
import { AppRoutingModule } from "./app.routing";
import { ComponentsModule } from "./components/components.module";
import { AppComponent } from "./app.component";
import { AdminLayoutComponent } from "./layouts/admin-layout/admin-layout.component";
import { ContasComponent } from "./contas/contas.component";
import { ConfiguracoesComponent } from "./configuracoes/configuracoes.component";
import { MatSnackBarModule } from "@angular/material/snack-bar";
import { MatInputModule } from "@angular/material/input";
import { NgxMaskModule } from "ngx-mask";
import { MatSelectModule } from "@angular/material/select";
import { CadastrarContaComponent } from "./contas/cadastrar-conta/cadastrar-conta.component";
import { MatFormFieldModule } from "@angular/material/form-field";
import { PipesModule } from "./core/pipe/pipes.module";

@NgModule({
  imports: [
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    ComponentsModule,
    RouterModule,
    AppRoutingModule,
    MatSnackBarModule,
    MatInputModule,
    MatSelectModule,
    NgxMaskModule.forRoot(),
    MatFormFieldModule,
    PipesModule
  ],
  declarations: [AppComponent, AdminLayoutComponent, ContasComponent, ConfiguracoesComponent, CadastrarContaComponent],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
