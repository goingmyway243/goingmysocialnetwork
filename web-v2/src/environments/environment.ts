export const environment = {
  production: false,
  apiUrl: 'http://localhost:5133/api',
  auth: {
    issuer: 'http://localhost:5133',
    clientId: 'web-client',
    redirectUri: window.location.origin + '/signin-oidc',
    postLogoutRedirectUri: window.location.origin + '/',
    responseType: 'code',
    scope: 'openid profile email roles',
    showDebugInformation: true,
    requireHttps: false,
    useSilentRefresh: false,
    silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
  }
};
