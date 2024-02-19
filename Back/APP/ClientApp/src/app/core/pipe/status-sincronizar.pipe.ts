import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
  name: "SincStatus",
})
export class SincStatusPipe implements PipeTransform {
  transform(valor: number): string {
    if (valor == 1) {
      return "PROCESSANDO";
    }
    
    if (valor == 2) {
      return "CONCLUÍDO";
    }

    return "ERRO";
  }
}
