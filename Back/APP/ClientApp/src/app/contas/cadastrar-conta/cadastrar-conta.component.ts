import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatSnackBar } from "@angular/material/snack-bar";
import { Router } from "@angular/router";
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
  public carregando: boolean = false;
  public form: FormGroup;
  public projetosPrincipal: TeamProjectReference[] = [];
  public projetosSecundario: TeamProjectReference[] = [];
  public times: TeamProjectReference[] = [];
  public areas: TeamProjectReference[] = [];
  public textoCarregando: string = "Salvando";

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
  }

  private _formularioCad(): void {
    this.form = this._formBuilder.group({
      tokenPrincipal: ["", [Validators.maxLength(255), Validators.required]],
      urlPrincipal: ["", [Validators.maxLength(255), Validators.required]],
      projetoPrincipal: ["", [Validators.required]],
      nomePrincipal: ["", [Validators.required]],
      tokenSecundario: ["", [Validators.required]],
      urlSecundario: ["", [Validators.required]],
      nomeSecundario: ["", [Validators.required]],
      projetoSecundario: ["", [Validators.required]],
      time: ["", [Validators.required]],
      areaPath: ["", [Validators.required]],
    });
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
      } else {
        this.projetosSecundario = res.projetos;
      }
      this.textoCarregando = "Salvando";
    } catch (error) {
      this.carregando = false;
      this.textoCarregando = "Salvando";
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
      this.textoCarregando = "Salvando";
    } catch (error) {
      this.carregando = false;
      this.textoCarregando = "Salvando";
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
      this.textoCarregando = "Salvando";
    } catch (error) {
      this.carregando = false;
      this.textoCarregando = "Salvando";
    }
  }
}
