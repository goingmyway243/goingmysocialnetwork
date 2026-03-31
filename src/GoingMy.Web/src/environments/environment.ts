export const environment = {
  production: false,
  blazorLoginUrl: 'https://localhost:7001/login',
  authConfig: {
    issuer: 'https://localhost:7001/',
    clientId: 'web-client',
    redirectUri: window.location.origin + '/signin-oidc',
    postLogoutRedirectUri: window.location.origin + '/',
    responseType: 'code',
    scope: 'social_api',
    showDebugInformation: true,
    useSilentRefresh: false,
    silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
  }
};
