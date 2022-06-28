import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SliderComponent } from './components/slider/slider.component';
import { DropDownComponent } from './components/drop-down/drop-down.component';
import { InputSmallComponent } from './components/input-small/input-small.component';
import { InputMediumComponent } from './components/input-medium/input-medium.component';
import { InputLargeComponent } from './components/input-large/input-large.component';
import { ModalComponent } from './components/modal/modal.component';
import { ColorInputComponent } from './components/color-input/color-input.component';
import { InputLargeDirective } from './components/input-large/input-large.directive';
import { InputSmallDirective } from './components/input-small/input-small.directive';
import { PaneModule } from './components/pane-container/pane.module';
import { ModalService } from './components/modal/modal.service';
import { FormsModule } from '@angular/forms';
import { InputMediumDirective } from './components/input-medium/input-medium.directive';
import { HelperService } from './components/helper.service';
import { ButtonDirective } from './components/button/button.directive';
import { ButtonSmallDirective } from './components/button/button-small.directive';
import { RgbToStringPipe } from './components/color-input/rgb-to-string.pipe';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@NgModule({
  declarations: [
    SliderComponent,
    DropDownComponent,
    InputSmallComponent,
    InputMediumComponent,
    InputLargeComponent,
    ModalComponent,
    ColorInputComponent,
    InputSmallDirective,
    InputMediumDirective,
    InputLargeDirective,
    ButtonSmallDirective,
    ButtonDirective,
    RgbToStringPipe
  ],
  imports: [CommonModule, FormsModule, FontAwesomeModule],
  exports: [
    SliderComponent,
    DropDownComponent,
    InputSmallComponent,
    InputMediumComponent,
    InputLargeComponent,
    ModalComponent,
    ColorInputComponent,
    InputSmallDirective,
    InputMediumDirective,
    InputLargeDirective,
    PaneModule,
    ButtonSmallDirective,
    ButtonDirective
  ],
  providers: [ModalService, HelperService, RgbToStringPipe]
})
export class SharedModule {}
