import { asiaEntity, asiaGuid } from './entity.model';
export class asiaUser extends asiaEntity {
  email: string;
  lastName: string;
  firstName: string;
  hash: string;
  constructor(email: string, lastName: string, firstName: string, hash: string, id?: asiaGuid) {
    super(id);
    this.email = email;
    this.lastName = lastName;
    this.firstName = firstName;
    this.hash = hash;
  }

  // if eslint complains about a parsing error here just ignore it.
  static override copy(user: asiaUser) {
    return new asiaUser(user.email, user.lastName, user.firstName, user.hash, user.id);
  }
}
