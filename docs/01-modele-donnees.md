# Modèle des données collectées

## 1. Identité

Une identité représente un compte provenant de Microsoft Entra ID ou d’Active Directory On-Premise.

| Champ            | Type             | Description                                        |
| ---------------- | ---------------- | -------------------------------------------------- |
| Id               | UUID             | Identifiant interne de l’application               |
| ExternalId       | Texte            | Identifiant de l’objet dans l’annuaire             |
| DisplayName      | Texte            | Nom d’affichage                                    |
| UserName         | Texte            | Nom de connexion                                   |
| Email            | Texte nullable   | Adresse électronique                               |
| Source           | Enum             | EntraID ou ActiveDirectory                         |
| AccountType      | Enum             | User, Guest, Administrator ou ServiceAccount       |
| IsEnabled        | Booléen          | Indique si le compte est actif                     |
| IsPrivileged     | Booléen          | Indique si le compte possède des privilèges élevés |
| IsServiceAccount | Booléen          | Indique s’il s’agit d’un compte de service         |
| IsLocked         | Booléen nullable | Indique si le compte est verrouillé                |
| LastSignInAt     | Date nullable    | Date de dernière connexion                         |
| Description      | Texte nullable   | Description du compte                              |
| Owner            | Texte nullable   | Propriétaire ou responsable du compte              |
| TargetId         | UUID             | Cible dont provient le compte                      |
| CollectedAt      | Date             | Date de collecte                                   |

## 2. Groupe d’annuaire

Un groupe représente un groupe provenant de Microsoft Entra ID ou d’Active Directory.

| Champ        | Type           | Description                                     |
| ------------ | -------------- | ----------------------------------------------- |
| Id           | UUID           | Identifiant interne                             |
| ExternalId   | Texte          | Identifiant du groupe dans l’annuaire           |
| Name         | Texte          | Nom du groupe                                   |
| Description  | Texte nullable | Description du groupe                           |
| Source       | Enum           | EntraID ou ActiveDirectory                      |
| GroupType    | Texte nullable | Type du groupe                                  |
| IsPrivileged | Booléen        | Indique si le groupe est sensible ou privilégié |
| TargetId     | UUID           | Cible associée                                  |
| CollectedAt  | Date           | Date de collecte                                |

## 3. Appartenance à un groupe

Cette entité permet de représenter la relation entre une identité et un groupe.

| Champ          | Type  | Description          |
| -------------- | ----- | -------------------- |
| Id             | UUID  | Identifiant interne  |
| IdentityId     | UUID  | Identité membre      |
| GroupId        | UUID  | Groupe concerné      |
| MembershipType | Texte | Directe ou indirecte |
| CollectedAt    | Date  | Date de collecte     |

## 4. Rôle d’annuaire

Un rôle représente une fonction ou un niveau d’autorisation dans Microsoft Entra ID ou Active Directory.

| Champ        | Type           | Description                         |
| ------------ | -------------- | ----------------------------------- |
| Id           | UUID           | Identifiant interne                 |
| ExternalId   | Texte          | Identifiant du rôle dans l’annuaire |
| Name         | Texte          | Nom du rôle                         |
| Description  | Texte nullable | Description du rôle                 |
| Source       | Enum           | EntraID ou ActiveDirectory          |
| IsPrivileged | Booléen        | Indique si le rôle est sensible     |
| TargetId     | UUID           | Cible associée                      |
| CollectedAt  | Date           | Date de collecte                    |

## 5. Attribution de rôle

Cette entité représente l’attribution d’un rôle à une identité.

| Champ           | Type          | Description                          |
| --------------- | ------------- | ------------------------------------ |
| Id              | UUID          | Identifiant interne                  |
| IdentityId      | UUID          | Identité concernée                   |
| DirectoryRoleId | UUID          | Rôle attribué                        |
| AssignedAt      | Date nullable | Date d’attribution                   |
| ExpiresAt       | Date nullable | Date d’expiration                    |
| IsPermanent     | Booléen       | Attribution permanente ou temporaire |
| CollectedAt     | Date          | Date de collecte                     |

## 6. Principes de normalisation

Les données provenant d’Entra ID et d’Active Directory seront converties vers un modèle commun.

Exemples :

- un utilisateur Entra ID et un utilisateur Active Directory seront stockés dans la table `Identities` ;
- un groupe Entra ID et un groupe Active Directory seront stockés dans `DirectoryGroups` ;
- les différences propres à chaque environnement seront stockées dans des champs optionnels ;
- chaque objet conservera sa source et son identifiant externe ;
- les collecteurs fonctionneront uniquement en lecture sur les annuaires.
