import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44318/',
  redirectUri: baseUrl,
  clientId: 'SystemAdministration_App',
  responseType: 'code',
  scope: 'offline_access SystemAdministration',
  requireHttps: true
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'SystemAdministration',
    logoUrl: '',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44318',
      rootNamespace: 'HQSOFT.SystemAdministration',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
    SystemAdministration: {
      url: 'https://localhost:44313',
      rootNamespace: 'HQSOFT.SystemAdministration',
    },
  },
} as Environment;
