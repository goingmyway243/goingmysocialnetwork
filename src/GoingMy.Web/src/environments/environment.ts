export const environment = {
  production: false,
  authConfig: {
    issuer: 'https://localhost:7001/',
    clientId: 'web-client',
    redirectUri: window.location.origin + '/signin-oidc',
    postLogoutRedirectUri: window.location.origin + '/',
    responseType: 'code',
    scope: 'openid profile email roles',
    showDebugInformation: true,
    useSilentRefresh: false,
    silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
  }
};
