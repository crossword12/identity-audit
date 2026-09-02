# Identity Audit — Frontend

Interface React de la plateforme Identity Audit consacrée à l’audit des identités et des privilèges selon les CIS Controls 5 et 6.

## Stack

- React 19 ;
- TypeScript 6 ;
- Vite 8 ;
- React Router DOM 7 ;
- Axios ;
- Lucide React ;
- CSS.

## Authentification et autorisation

L’interface utilise l’authentification JWT fournie par l’API.

Le jeton est automatiquement ajouté aux appels protégés :

```http
Authorization: Bearer <JWT>
```

Trois rôles sont disponibles :

| Rôle            | Accès                                            |
| --------------- | ------------------------------------------------ |
| `Reader`        | Consultation des audits et résultats             |
| `Auditor`       | Consultation et gestion des audits               |
| `Administrator` | Gestion complète, utilisateurs et journal global |

Les routes d’administration sont protégées à la fois dans le frontend et dans l’API.

## Fonctionnalités

### Connexion

- authentification par adresse électronique et mot de passe ;
- récupération du profil connecté ;
- conservation contrôlée de la session ;
- déconnexion ;
- redirection vers la page de connexion en cas de session invalide.

### Tableau de bord

- sélection de la cible analysée ;
- affichage du dernier audit terminé ;
- nombre d’identités et de comptes privilégiés ;
- nombre de constats ;
- score de conformité CIS ;
- répartition des constats par gravité ;
- distinction CIS Control 5 et CIS Control 6 ;
- affichage des audits récents.

### Cibles

- liste des cibles ;
- distinction Microsoft Entra ID et Active Directory ;
- création et modification ;
- activation et désactivation ;
- validation de la configuration JSON ;
- test de connexion.

Le test depuis cette page reste simulé. La connexion Active Directory réelle est effectuée par le collecteur Windows avec LDAPS.

### Audits

- historique complet ;
- filtrage par statut ;
- création d’un audit ;
- sélection d’une cible active ;
- démarrage d’un audit en attente ;
- finalisation d’un audit en cours ;
- affichage du score CIS ;
- accès au détail.

### Détail d’un audit

La page est organisée en cinq onglets :

- **Résultats CIS** : statuts, sévérités, constats, preuves et recommandations ;
- **Identités** : comptes, états, types et privilèges ;
- **Groupes** : groupes d’annuaire et appartenances ;
- **Rôles** : rôles, privilèges et affectations ;
- **Journal** : historique des opérations réalisées sur l’audit.

Les résultats CIS peuvent être téléchargés en CSV ou sous la forme d’un rapport PDF détaillé.

### Utilisateurs

Cette page est réservée aux administrateurs.

Elle permet :

- de consulter les utilisateurs ;
- de créer un compte ;
- de modifier un compte ;
- d’activer ou désactiver un compte ;
- de gérer ses rôles.

### Journal d’activité

Cette page est réservée aux administrateurs.

Elle affiche les événements de tous les audits avec :

- date ;
- type d’événement ;
- niveau ;
- utilisateur ;
- audit concerné ;
- message ;
- recherche ;
- filtre par niveau ;
- lien vers le détail de l’audit.

## Routes

| Route              | Page               | Accès           |
| ------------------ | ------------------ | --------------- |
| `/login`           | Connexion          | Public          |
| `/`                | Tableau de bord    | Authentifié     |
| `/targets`         | Cibles             | Authentifié     |
| `/audits`          | Audits             | Authentifié     |
| `/audits/:auditId` | Détail d’un audit  | Authentifié     |
| `/users`           | Utilisateurs       | `Administrator` |
| `/activity-logs`   | Journal d’activité | `Administrator` |

## Structure du code

```text
src/
├── api/
│   ├── auditLogsApi.ts
│   ├── auditsApi.ts
│   ├── authenticationApi.ts
│   ├── dashboardApi.ts
│   ├── directoryAccessApi.ts
│   ├── httpClient.ts
│   ├── identitiesApi.ts
│   ├── ruleEvaluationsApi.ts
│   ├── targetsApi.ts
│   └── usersApi.ts
├── auth/
├── components/
│   ├── audit-details/
│   ├── audits/
│   ├── layout/
│   ├── targets/
│   └── users/
├── pages/
│   ├── ActivityLogs/
│   ├── AuditDetails/
│   ├── Audits/
│   ├── Dashboard/
│   ├── Login/
│   ├── Targets/
│   └── Users/
├── routes/
├── types/
├── App.tsx
├── index.css
└── main.tsx
```

## Prérequis

- Node.js 24 recommandé ;
- npm 11 ou supérieur ;
- API Identity Audit disponible.

## Installation

Depuis la racine du dépôt :

```bash
npm --prefix frontend install
```

Créer la configuration locale :

```bash
cp frontend/.env.example frontend/.env
```

Contenu :

```env
VITE_API_BASE_URL=http://localhost:5173
```

## Lancement

Depuis la racine :

```bash
npm --prefix frontend run dev
```

Le frontend utilise :

```text
http://localhost:5174
```

L’API doit être disponible sur :

```text
http://localhost:5173
```

## Communication avec l’API

Les appels HTTP sont centralisés dans :

```text
src/api/httpClient.ts
```

Ce client :

- utilise `VITE_API_BASE_URL` ;
- ajoute automatiquement le JWT ;
- centralise les traitements liés à l’authentification.

Les services sont séparés par domaine fonctionnel.

## Organisation des styles

Les variables globales et la charte graphique sont définies dans :

```text
src/index.css
```

Les pages et composants possèdent leurs propres fichiers CSS afin de limiter les conflits.

## Charte graphique

| Usage            | Couleur   |
| ---------------- | --------- |
| Principale       | `#1F4E78` |
| Secondaire       | `#2F75B5` |
| Fond général     | `#F5F7FA` |
| Cartes           | `#FFFFFF` |
| Texte principal  | `#1F2937` |
| Texte secondaire | `#6B7280` |
| Succès           | `#22C55E` |
| Danger           | `#DC2626` |
| Avertissement    | `#F59E0B` |

## Scripts

| Commande          | Description                             |
| ----------------- | --------------------------------------- |
| `npm run dev`     | Démarrer Vite                           |
| `npm run build`   | Vérifier TypeScript et générer le build |
| `npm run lint`    | Analyser le code                        |
| `npm run preview` | Prévisualiser le build                  |

Depuis la racine du dépôt :

```bash
npm --prefix frontend run build
```

Les fichiers générés sont placés dans :

```text
frontend/dist/
```

## Vérifications recommandées

Avant une livraison :

```bash
npm --prefix frontend run lint
npm --prefix frontend run build
```

Vérifier manuellement :

- connexion et déconnexion ;
- restrictions selon les rôles ;
- tableau de bord ;
- gestion des cibles ;
- cycle des audits ;
- cinq onglets du détail ;
- exports CSV et PDF ;
- journal d’un audit ;
- gestion des utilisateurs ;
- journal d’activité global ;
- recherche et filtres ;
- affichage sur différentes tailles d’écran.
