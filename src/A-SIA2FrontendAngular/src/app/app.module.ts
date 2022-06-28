import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DataPanelModule } from './modules/data-panel/data-panel.module';
import { GraphEditorModule } from './modules/graph-editor/graph-editor.module';
import { MenubarModule } from './modules/menubar/menubar.module';
import { SimulationDiagramModule } from './modules/simulation-diagram/simulation-diagram.module';
import { SharedModule } from './shared/shared.module';
import { LoginComponent } from './modules/login/login.component';
import { CookieService } from 'ngx-cookie-service';
import { JwtHelperService, JWT_OPTIONS } from '@auth0/angular-jwt';
import { JwtInterceptor } from './interceptors/jwt.interceptor';
import { ErrorInterceptor } from './interceptors/error.interceptor';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { SettingsComponent } from './modules/settings/settings.component';
import { NetworkViewComponent } from './modules/network-view/network-view.component';
import { NetworkExplorerModule } from './modules/network-explorer/network-explorer.module';

@NgModule({
  declarations: [AppComponent, LoginComponent, SettingsComponent, NetworkViewComponent],
  imports: [
    BrowserModule,
    AppRoutingModule,
    DataPanelModule,
    MenubarModule,
    GraphEditorModule,
    SimulationDiagramModule,
    SharedModule,
    FormsModule,
    HttpClientModule,
    NetworkExplorerModule,
    FontAwesomeModule
  ],
  providers: [
    { provide: JWT_OPTIONS, useValue: JWT_OPTIONS },
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    CookieService,
    JwtHelperService
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
