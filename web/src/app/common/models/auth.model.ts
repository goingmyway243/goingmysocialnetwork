export interface ILoginRequest {
    email: string;
    password: string;
}

export interface IRegisterRequest {
    email: string;
    password: string;
    fullName: string;
    dateOfBirth: Date;
}

export interface IRegisterResponse {
    userId: string;
    userName: string;
}