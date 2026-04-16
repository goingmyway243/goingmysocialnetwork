export const environment = {
  production: true,
  apiGatewayUrl: 'https://api.yourdomain.com',
  auth: {
    issuer: 'https://identity.yourdomain.com',
    clientId: 'web-client',
    redirectUri: window.location.origin + '/signin-oidc',
    postLogoutRedirectUri: window.location.origin + '/',
    responseType: 'code',
    scope: 'openid profile email roles offline_access',
    showDebugInformation: false,
    requireHttps: true,
    useSilentRefresh: true,
    silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
  }
};
