import { User } from "../models/user.model";

export interface ILoginRequest {
    email: string;
    password: string;
}

export interface ILoginResponse {
    user: User;
    token: string;
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