import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatSnackBar } from "@angular/material/snack-bar";
import { Configuracao } from "app/core/models/configuracao";
import { ConfiguracoesControllerService } from "app/core/services/ConfiguracoesController.service";
import { ContasControllerService } from "app/core/services/ContasController.service";

@Component({
  selector: "app-configuracoes",
  templateUrl: "./configuracoes.component.html",
  styleUrls: ["./configuracoes.component.scss"],
})
export class ConfiguracoesComponent implements OnInit {
  public carregando: boolean = false;
  public configuracao: Configuracao;
  public form: FormGroup;

  constructor(
    private _configuracoesControllerService: ConfiguracoesControllerService,
    private _formBuilder: FormBuilder,
    private _snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this._formularioCad();
    this._buscarConfig();
  }

  private _formularioCad(): void {
    this.form = this._formBuilder.group({
      dia: [this.configuracao?.dia, [Validators.maxLength(255), Validators.required]],
      cliente: [this.configuracao?.cliente, [Validators.maxLength(255)]],
      horaCron: [this.configuracao?.horaCron, [Validators.maxLength(10), Validators.required]],

      email: [this.configuracao?.email, [Validators.maxLength(255)]],
      senha: [this.configuracao?.senha, [Validators.maxLength(255)]],
      smtp: [this.configuracao?.smtp, [Validators.maxLength(255)]],
      porta: [this.configuracao?.porta, []],
    });
  }

  private async _buscarConfig(): Promise<void> {
    this.carregando = true;
    try {
      const res = await this._configuracoesControllerService.lista().toPromise();
      this.carregando = false;
      if (res.dados.length > 0) {
        this.configuracao = res.dados[0];
        this.form.get("dia").setValue(this.configuracao.dia);
        this.form.get("horaCron").setValue(this.configuracao.horaCron);
        this.form.get("cliente").setValue(this.configuracao.cliente);

        this.form.get("email").setValue(this.configuracao.email);
        this.form.get("senha").setValue(this.configuracao.senha);
        this.form.get("smtp").setValue(this.configuracao.smtp);
        this.form.get("porta").setValue(this.configuracao.porta);
      }
    } catch (error) {
      this.carregando = false;
      console.error(error);
    }
  }

  public async salvar(): Promise<void> {
    if (this.form.invalid) {
      return;
    }
    this.carregando = true;
    try {
      let dados = this.form.value as Configuracao;
      dados.id = this.configuracao.id;
      await this._configuracoesControllerService.atualizar([dados]).toPromise();
      this.carregando = false;
      this._snackBar.open("Configurações atualizadas com sucesso!", "Fechar", {
        duration: 3000,
      });
    } catch (error) {
      this.carregando = false;
      this._snackBar.open("Erro:" + error, "Fechar", {
        duration: 3000,
      });
      console.error(error);
    }
  }
}
