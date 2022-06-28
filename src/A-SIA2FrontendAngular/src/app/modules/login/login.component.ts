import { Component, OnInit } from '@angular/core';
import { AuthorizationService } from '@shared/backend/authorization.service';
import { ModalService } from '@shared/components/modal/modal.service';
@Component({
  selector: 'a-sia-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  constructor(private authorizationService: AuthorizationService, private modalService: ModalService) {}

  ngOnInit(): void {
    if (this.authorizationService.loggedIn !== true) this.modalService.openModal('login');
  }

  username = 'test@test';
  password = '1234';

  onLogin = (): void => {
    this.authorizationService.login(this.username, this.password).subscribe((data) => {
      if (data.status == 200 && data.body) {
        this.authorizationService.loggedIn = true;
        this.authorizationService.setToken(data.body);
        console.log(`Logged in: ${this.authorizationService.loggedIn}`);
        this.modalService.closeModal('login');
        location.reload();
      }
    });
  };
}
