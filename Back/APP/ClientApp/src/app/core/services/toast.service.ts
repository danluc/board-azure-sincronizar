import { Injectable } from "@angular/core";
import { MatSnackBar, MatSnackBarHorizontalPosition, MatSnackBarVerticalPosition } from "@angular/material/snack-bar";

@Injectable({
  providedIn: "root",
})
export class ToastService {
  _defaultHorizPosition: MatSnackBarHorizontalPosition = "right";
  _defaultVertPosition: MatSnackBarVerticalPosition = "top";
  _defaultDuration: number = 5000;

  constructor(private _snackBar: MatSnackBar) {}

  mensagemInfo(
    msg: string,
    duration: number = this._defaultDuration,
    btn: string = "OK",
    horizontalPosition: MatSnackBarHorizontalPosition = this._defaultHorizPosition,
    verticalPosition: MatSnackBarVerticalPosition = this._defaultVertPosition,
    panelClass: string[] = ["toast-info"]
  ): void {
    this._snackBar.open(msg, btn, {
      duration: duration,
      horizontalPosition: horizontalPosition,
      verticalPosition: verticalPosition,
      panelClass: panelClass,
    });
  }

  mensagemError(
    msg: string,
    duration: number = this._defaultDuration,
    btn: string = "OK",
    horizontalPosition: MatSnackBarHorizontalPosition = this._defaultHorizPosition,
    verticalPosition: MatSnackBarVerticalPosition = this._defaultVertPosition
  ): void {
    this.mensagemInfo(msg, duration, btn, horizontalPosition, verticalPosition, ["toast-error"]);
  }

  mensagemWarning(
    msg: string,
    duration: number = this._defaultDuration,
    btn: string = "OK",
    horizontalPosition: MatSnackBarHorizontalPosition = this._defaultHorizPosition,
    verticalPosition: MatSnackBarVerticalPosition = this._defaultVertPosition
  ): void {
    this.mensagemInfo(msg, duration, btn, horizontalPosition, verticalPosition, ["toast-warning"]);
  }

  mensagemSuccess(
    msg: string,
    duration: number = this._defaultDuration,
    btn: string = "OK",
    horizontalPosition: MatSnackBarHorizontalPosition = this._defaultHorizPosition,
    verticalPosition: MatSnackBarVerticalPosition = this._defaultVertPosition
  ): void {
    this.mensagemInfo(msg, duration, btn, horizontalPosition, verticalPosition, ["toast-success"]);
  }

  atualizarToast(): void {
    const snack = document.body.getElementsByClassName("toast-success");
    const snackBar = document.getElementById("toast-success");
    snackBar.click();
    const content = snackBar.innerHTML;
    snackBar.innerHTML = content;
  }
}
