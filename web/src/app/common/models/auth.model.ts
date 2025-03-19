export class LoginRequest {
    email: string = '';
    password: string = '';
}

export class RegisterRequest {
    email: string = '';
    password: string = '';
    fullName: string = '';
}

export class RegisterResponse {
    userId: string = '';
    email: string = '';
}