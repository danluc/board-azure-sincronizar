import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { SincStatusPipe } from "./status-sincronizar.pipe";

@NgModule({
  imports: [CommonModule],
  declarations: [SincStatusPipe],
  exports: [SincStatusPipe],
})
export class PipesModule {}
