export const environment = {
  production: false,
  apiGatewayUrl: 'https://localhost:7000',
  authConfig: {
    issuer: 'https://localhost:7001/',
    clientId: 'web-client',
    redirectUri: window.location.origin + '/signin-oidc',
    postLogoutRedirectUri: window.location.origin + '/',
    logoutUrl: 'https://localhost:7001/connect/logout',  // OpenIddict logout endpoint
    responseType: 'code',
    scope: 'openid profile email roles social_api',
    showDebugInformation: true,
    requireHttps: false,  // Local development only
    strictDiscoveryDocumentValidation: false,  // Local development only
    // PKCE is enabled automatically when responseType is 'code'
  }
};
