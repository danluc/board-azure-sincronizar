import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatSnackBar } from "@angular/material/snack-bar";
import { Azure } from "app/core/models/azure";
import { ListaSprintsDTO } from "app/core/models/lista-sprintsDTO";
import { BuscarProjetoDTO, TeamProjectReference } from "app/core/models/team-project-reference";
import { AzureControllerService } from "app/core/services/AzureController.service";
import { ProjetosControllerService } from "app/core/services/ProjetosController.service";
import { TimesControllerService } from "app/core/services/TimesController.service";

@Component({
  selector: "app-cadastrar-azure",
  templateUrl: "./cadastrar-azure.component.html",
  styleUrls: ["./cadastrar-azure.component.scss"],
})
export class CadastrarAzureComponent implements OnInit {
  public cadastrar: boolean = true;
  public carregando: boolean = false;
  public form: FormGroup;
  public projetosPrincipal: TeamProjectReference[] = [];
  public projetosSecundario: TeamProjectReference[] = [];
  public times: TeamProjectReference[] = [];
  public areas: TeamProjectReference[] = [];
  public sprints: ListaSprintsDTO[] = [];
  public contaAtualizar: Azure[] = [];
  public textoCarregando: string = "Buscando azure...";

  constructor(
    private _azureControllerService: AzureControllerService,
    private _projetosControllerService: ProjetosControllerService,
    private _timesControllerService: TimesControllerService,
    private _formBuilder: FormBuilder,
    private _snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this._formularioCad();
    this._buscarContasAzure();
  }

  private _formularioCad(): void {
    this.form = this._formBuilder.group({
      tokenPrincipal: ["", [Validators.required]],
      urlPrincipal: ["", [Validators.required]],
      projetoPrincipal: ["", [Validators.required]],

      tokenSecundario: ["", [Validators.required]],
      urlSecundario: ["", [Validators.required]],
      projetoSecundario: ["", [Validators.required]],
      timeSecundario: ["", [Validators.required]],
    });
  }

  private async _buscarContasAzure(): Promise<void> {
    this.carregando = true;
    const res = await this._azureControllerService.lista().toPromise();

    this.carregando = false;
    if (res.length == 0) {
      return;
    }
    this.contaAtualizar = res;

    this.cadastrar = false;
    this.form.get("tokenPrincipal").setValue(this.contaAtualizar[0].token);
    this.form.get("urlPrincipal").setValue(this.contaAtualizar[0].urlCorporacao);
    this.buscarProjetos();

    this.form.get("tokenSecundario").setValue(this.contaAtualizar[1].token);
    this.form.get("urlSecundario").setValue(this.contaAtualizar[1].urlCorporacao);

    this.buscarProjetos(false);
  }

  public async buscarProjetos(principal: boolean = true): Promise<void> {
    let url = "";
    let token = "";

    if (principal) {
      url = this.form.get("urlPrincipal").value;
      token = this.form.get("tokenPrincipal").value;
    } else {
      url = this.form.get("urlSecundario").value;
      token = this.form.get("tokenSecundario").value;
    }

    if (url.length <= 1 || token.length <= 1) {
      return;
    }

    this.carregando = true;
    this.textoCarregando = "Buscando projetos...";

    try {
      let dados: BuscarProjetoDTO = { token: token, url: url };
      const res = await this._projetosControllerService.lista(dados).toPromise();
      this.carregando = false;

      if (principal) {
        this.projetosPrincipal = res;

        if (!this.cadastrar) {
          let p = res.find((e) => e.id == this.contaAtualizar[0].projetoId);
          this.form.get("projetoPrincipal").setValue(p);
        }
      } else {
        this.projetosSecundario = res;
        if (!this.cadastrar) {
          let ps = res.find((e) => e.id == this.contaAtualizar[1].projetoId);
          this.form.get("projetoSecundario").setValue(ps);
          this.buscarTimes();
        }
      }
    } catch (error) {
      this.carregando = false;
    }
  }

  public async buscarTimes(): Promise<void> {
    let url = this.form.get("urlSecundario").value;
    let token = this.form.get("tokenSecundario").value;
    let projeto = this.form.get("projetoSecundario").value;
    if (url.length <= 1 || token.length <= 1 || projeto?.name?.length <= 1) {
      return;
    }
    this.carregando = true;
    this.textoCarregando = "Buscando times...";

    try {
      let dados: BuscarProjetoDTO = { token: token, url: url };

      const res = await this._timesControllerService.lista(projeto.name, dados).toPromise();
      this.carregando = false;
      this.times = res;
      if (!this.cadastrar) {
        let ps = res.find((e) => e.id == this.contaAtualizar[1].timeId);
        this.form.get("timeSecundario").setValue(ps);
      }
    } catch (error) {
      this.carregando = false;
    }
  }

  private get _montarObj(): Azure[] {
    let contas: Azure[] = [
      {
        urlCorporacao: this.form.get("urlPrincipal").value,
        token: this.form.get("tokenPrincipal").value,
        projetoId: this.form.get("projetoPrincipal").value?.id,
        projetoNome: this.form.get("projetoPrincipal").value?.name,
        principal: true,
      },
      {
        urlCorporacao: this.form.get("urlSecundario").value,
        token: this.form.get("tokenSecundario").value,
        projetoId: this.form.get("projetoSecundario").value?.id,
        timeId: this.form.get("timeSecundario").value?.id,
        projetoNome: this.form.get("projetoSecundario").value?.name,
        timeNome: this.form.get("timeSecundario").value?.name,
        principal: false,
      },
    ];
    return contas;
  }

  public async salvar(): Promise<void> {
    this.carregando = true;
    this.textoCarregando = "Salvando...";
    if (this.cadastrar) {
      await this._cadastrar();
    } else {
      await this._atualizar();
    }
  }

  private async _cadastrar(): Promise<void> {
    try {
      const contas = this._montarObj;
      const res = await this._azureControllerService.cadastrar(contas).toPromise();
      this.carregando = false;
      this._snackBar.open("Conta Azure cadastrada com sucesso!", "Fechar", {
        duration: 3000,
      });
    } catch (error) {
      this.carregando = false;
      this._snackBar.open("Erro para cadastrar conta!", "Fechar", {
        duration: 5000,
      });
    }
  }

  private async _atualizar(): Promise<void> {
    try {
      const contas = this._montarObj;
      contas[0].id = this.contaAtualizar[0].id
      contas[1].id = this.contaAtualizar[1].id
      const res = await this._azureControllerService.atualizar(contas).toPromise();
      this.carregando = false;
      this._snackBar.open("Conta Azure atualizadas com sucesso!", "Fechar", {
        duration: 3000,
      });
    } catch (error) {
      this.carregando = false;
      this._snackBar.open("Erro ao atualizar conta!", "Fechar", {
        duration: 5000,
      });
    }
  }
}
