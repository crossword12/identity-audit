# Identity Audit

Outil d’audit des identités et des privilèges aligné sur les CIS Controls 5 et 6.

## Objectif

L’application permet de collecter, stocker, analyser et visualiser les informations liées aux identités, groupes, rôles et privilèges provenant de :

- Microsoft Entra ID ;
- Active Directory On-Premise.

## Stack technique

- Backend : ASP.NET Core 10 / C#
- Base de données : PostgreSQL
- Frontend : React avec TypeScript
- Collecteurs : C#
- Communication : API REST / JSON
- Déploiement principal : Linux
- Versionnement : Git

## Structure du projet

```text
identity-audit/
├── backend/
├── frontend/
├── collectors/
│   ├── entra-id/
│   ├── active-directory/
│   └── mock-collector/
├── database/
├── deployment/
├── tests/
├── docs/
└── README.md
```
