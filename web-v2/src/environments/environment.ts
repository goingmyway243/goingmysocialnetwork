export const environment = {
  production: false,
  apiUrl: 'http://localhost:5133/api',
  authCodeFlowConfig: {
    issuer: 'http://localhost:5133',
    redirectUri: window.location.origin + '/signin-oidc',
    clientId: 'spa',
    responseType: 'code',
    scope: 'roles profile email',
    showDebugInformation: true,
  }
};
