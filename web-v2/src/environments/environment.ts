export const environment = {
  production: false,
  apiUrl: 'https://localhost:7119/api',
  authConfig: {
    issuer: 'https://localhost:7119/',
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
