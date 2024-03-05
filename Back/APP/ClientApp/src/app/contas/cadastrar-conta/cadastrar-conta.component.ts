import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatSnackBar } from "@angular/material/snack-bar";
import { Router } from "@angular/router";
import { Conta } from "app/core/models/conta";
import { BuscarIterationsDTO, BuscarProjetoDTO, TeamProjectReference } from "app/core/models/team-project-reference";
import { ContasControllerService } from "app/core/services/ContasController.service";
import { ProjetosControllerService } from "app/core/services/ProjetosController.service";
import { TimesControllerService } from "app/core/services/TimesController.service";

@Component({
  selector: "app-cadastrar-conta",
  templateUrl: "./cadastrar-conta.component.html",
  styleUrls: ["./cadastrar-conta.component.scss"],
})
export class CadastrarContaComponent implements OnInit {
  public cadastrar: boolean = true;
  public carregando: boolean = false;
  public form: FormGroup;
  public projetosPrincipal: TeamProjectReference[] = [];
  public projetosSecundario: TeamProjectReference[] = [];
  public times: TeamProjectReference[] = [];
  public areas: TeamProjectReference[] = [];
  public sprints: TeamProjectReference[] = [];
  public contaAtualizar: Conta[] = [];
  public textoCarregando: string = "Buscando contas...";

  constructor(
    private _contasControllerService: ContasControllerService,
    private _projetosControllerService: ProjetosControllerService,
    private _timesControllerService: TimesControllerService,
    private _formBuilder: FormBuilder,
    private _router: Router,
    private _snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this._formularioCad();
    this._buscarContas();
  }

  private _formularioCad(): void {
    this.form = this._formBuilder.group({
      tokenPrincipal: ["", [Validators.required]],
      urlPrincipal: ["", [Validators.required]],
      projetoPrincipal: ["", [Validators.required]],
      tokenSecundario: ["", [Validators.required]],
      urlSecundario: ["", [Validators.required]],
      nomeSecundario: ["", [Validators.required]],
      projetoSecundario: ["", [Validators.required]],
      time: ["", [Validators.required]],
      areaPath: ["", [Validators.required]],
      sprint: ["", [Validators.required]],
    });
  }

  private async _buscarContas(): Promise<void> {
    this.carregando = true;
    const res = await this._contasControllerService.lista().toPromise();
    this.carregando = false;
    if (res.dados.length == 0) {
      return;
    }
    this.contaAtualizar = res.dados;
    this.cadastrar = false;
    this.form.get("tokenPrincipal").setValue(this.contaAtualizar[0].token);
    this.form.get("urlPrincipal").setValue(this.contaAtualizar[0].urlCorporacao);
    this.buscarProjetos();

    this.form.get("tokenSecundario").setValue(this.contaAtualizar[1].token);
    this.form.get("urlSecundario").setValue(this.contaAtualizar[1].urlCorporacao);
    this.buscarProjetos(false);

    this.form.get("nomeSecundario").setValue(this.contaAtualizar[1].nomeUsuario);
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
        this.projetosPrincipal = res.projetos;

        if (!this.cadastrar) {
          let p = res.projetos.find((e) => e.id == this.contaAtualizar[0].projetoId);
          this.form.get("projetoPrincipal").setValue(p);
        }
      } else {
        this.projetosSecundario = res.projetos;

        if (!this.cadastrar) {
          let ps = res.projetos.find((e) => e.id == this.contaAtualizar[1].projetoId);
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
      this.times = res.dados;

      if (!this.cadastrar) {
        let ps = res.dados.find((e) => e.id == this.contaAtualizar[1].timeId);
        this.form.get("time").setValue(ps);
        this.buscarAreas();
      }
    } catch (error) {
      this.carregando = false;
    }
  }

  public async buscarAreas(): Promise<void> {
    let url = this.form.get("urlSecundario").value;
    let token = this.form.get("tokenSecundario").value;
    let projeto = this.form.get("projetoSecundario").value;
    let time = this.form.get("time").value;

    if (url.length <= 1 || token.length <= 1 || projeto?.name?.length <= 1 || time?.name?.length <= 1) {
      return;
    }

    this.carregando = true;
    this.textoCarregando = "Buscando areas...";

    try {
      let dados: BuscarIterationsDTO = { token: token, url: url, projetoNome: projeto.name, timeNome: time.name };
      const res = await this._timesControllerService.listaAreas(dados).toPromise();
      this.carregando = false;
      this.areas = res.dados;

      if (!this.cadastrar) {
        let ps = res.dados.find((e) => e.name == this.contaAtualizar[1].areaPath);
        this.form.get("areaPath").setValue(ps);
        this.buscarSprint();
      }
    } catch (error) {
      this.carregando = false;
    }
  }

  public async buscarSprint(): Promise<void> {
    let url = this.form.get("urlSecundario").value;
    let token = this.form.get("tokenSecundario").value;
    let projeto = this.form.get("projetoSecundario").value;
    let areaPath = this.form.get("areaPath").value;

    if (url.length <= 1 || token.length <= 1 || projeto?.name?.length <= 1 || areaPath?.name?.length <= 1) {
      return;
    }

    this.carregando = true;
    this.textoCarregando = "Buscando sprint...";
    this.sprints = [];
    try {
      let dados: BuscarIterationsDTO = { token: token, url: url, projetoNome: projeto.name, areaNome: areaPath.name };
      const res = await this._timesControllerService.listaSprint(dados).toPromise();
      this.carregando = false;
      this.sprints = res.dados;

      if (!this.cadastrar) {
        let ps = res.dados.find((e) => e.path == this.contaAtualizar[1].sprint);
        this.form.get("sprint").setValue(ps);
      }
    } catch (error) {
      this.carregando = false;
    }
  }

  private get _montarObj(): Conta[] {
    let contas: Conta[] = [
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
        areaPath: this.form.get("areaPath").value?.name,
        nomeUsuario: this.form.get("nomeSecundario").value,
        projetoId: this.form.get("projetoSecundario").value?.id,
        timeId: this.form.get("time").value?.id,
        projetoNome: this.form.get("projetoSecundario").value?.name,
        timeNome: this.form.get("time").value?.name,
        sprint: this.form.get("sprint").value?.path,
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
      const res = await this._contasControllerService.cadastrar(contas).toPromise();
      this.carregando = false;
      this._snackBar.open("Contas cadastrada com sucesso!", "Fechar", {
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
      contas[0].id = this.contaAtualizar[0].id;
      contas[1].id = this.contaAtualizar[1].id;
      const res = await this._contasControllerService.atualizar(contas).toPromise();
      this.carregando = false;
      this._snackBar.open("Contas atualizadas com sucesso!", "Fechar", {
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
