# Identity Audit — Frontend

Interface React de la plateforme Identity Audit dédiée à l’audit des identités et des privilèges selon les CIS Controls 5 et 6.

## Stack

- React 19 ;
- TypeScript 6 ;
- Vite 8 ;
- React Router DOM 7 ;
- Axios ;
- Lucide React ;
- CSS.

## Fonctionnalités

### Tableau de bord

- sélection de la cible analysée ;
- affichage du dernier audit terminé ;
- nombre d’identités et de comptes privilégiés ;
- nombre de constats ;
- score de conformité CIS ;
- répartition des constats par gravité ;
- distinction entre CIS Control 5 et CIS Control 6 ;
- affichage des trois audits les plus récents ;
- conservation des audits en attente dans l’historique.

### Cibles

- liste des cibles ;
- distinction entre Microsoft Entra ID et Active Directory ;
- affichage du statut et de la dernière collecte ;
- création d’une cible ;
- modification d’une cible ;
- activation et désactivation ;
- validation de la configuration JSON ;
- test de connexion.

### Audits

- historique complet ;
- filtrage par statut ;
- création d’un audit ;
- sélection d’une cible active ;
- démarrage d’un audit en attente ;
- finalisation d’un audit en cours ;
- affichage du score CIS ;
- accès au détail d’un audit.

### Détail d’un audit

La page de détail est organisée en quatre onglets :

- **Résultats CIS** : règles, statuts, sévérités, constats, preuves et recommandations ;
- **Identités** : comptes collectés, états, types et privilèges ;
- **Groupes** : groupes d’annuaire, groupes privilégiés et appartenances ;
- **Rôles** : rôles, privilèges et affectations.

## Routes

| Route              | Page               |
| ------------------ | ------------------ |
| `/`                | Tableau de bord    |
| `/targets`         | Gestion des cibles |
| `/audits`          | Gestion des audits |
| `/audits/:auditId` | Détail d’un audit  |

## Structure du code

```text
src/
├── api/
│   ├── auditsApi.ts
│   ├── dashboardApi.ts
│   ├── directoryAccessApi.ts
│   ├── httpClient.ts
│   ├── identitiesApi.ts
│   ├── ruleEvaluationsApi.ts
│   └── targetsApi.ts
├── components/
│   ├── audit-details/
│   ├── audits/
│   ├── layout/
│   └── targets/
├── pages/
│   ├── AuditDetails/
│   ├── Audits/
│   ├── Dashboard/
│   └── Targets/
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

Depuis le dossier `frontend` :

```bash
npm install
```

Créer le fichier d’environnement local :

```bash
cp .env.example .env
```

Contenu attendu :

```env
VITE_API_BASE_URL=http://localhost:5173
```

## Lancement en développement

```bash
npm run dev
```

Le serveur Vite utilise le port fixe :

```text
http://localhost:5174
```

L’API doit être disponible sur :

```text
http://localhost:5173
```

## Scripts

| Commande          | Description                             |
| ----------------- | --------------------------------------- |
| `npm run dev`     | Démarrer le serveur de développement    |
| `npm run build`   | Vérifier TypeScript et générer le build |
| `npm run lint`    | Analyser le code avec Oxlint            |
| `npm run preview` | Prévisualiser le build de production    |

## Communication avec l’API

Les appels HTTP sont centralisés dans :

```text
src/api/httpClient.ts
```

L’adresse de base provient de :

```env
VITE_API_BASE_URL
```

Les services API sont séparés par domaine :

- Dashboard ;
- cibles ;
- audits ;
- identités ;
- groupes et rôles ;
- évaluations des règles.

## Organisation des styles

Les variables globales et la charte graphique sont définies dans :

```text
src/index.css
```

Les styles spécifiques sont séparés par page ou composant :

```text
DashboardPage.css
TargetsPage.css
AuditsPage.css
AuditDetailsPage.css
```

Cette organisation limite les conflits entre les pages tout en conservant une charte graphique commune.

## Charte graphique

| Usage              | Couleur   |
| ------------------ | --------- |
| Couleur principale | `#1F4E78` |
| Couleur secondaire | `#2F75B5` |
| Fond général       | `#F5F7FA` |
| Cartes             | `#FFFFFF` |
| Texte principal    | `#1F2937` |
| Texte secondaire   | `#6B7280` |
| Succès             | `#22C55E` |
| Danger             | `#DC2626` |
| Avertissement      | `#F59E0B` |

## Build de production

```bash
npm run build
```

Les fichiers compilés sont générés dans :

```text
dist/
```

Pour les prévisualiser :

```bash
npm run preview
```

## Vérifications recommandées

Avant une livraison :

```bash
npm run lint
npm run build
```

Vérifier ensuite :

- le changement de cible dans le Dashboard ;
- la création et la modification d’une cible ;
- la création d’un audit ;
- le filtrage des audits ;
- l’accès aux détails ;
- l’affichage des identités, groupes et rôles ;
- l’adaptation de l’interface sur un écran étroit.
