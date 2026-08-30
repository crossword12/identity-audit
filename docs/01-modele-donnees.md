# Modèle des données collectées

## 1. Principe général

Les données collectées depuis Microsoft Entra ID et Active Directory On-Premise sont converties vers un modèle commun.

Chaque collecte est rattachée à un audit précis. Une même cible peut donc posséder plusieurs audits, et chaque audit conserve sa propre photographie des identités, groupes, rôles et relations collectés.

Le lien avec la cible est obtenu de manière indirecte :

```text
Target
  ↓
Audit
  ↓
Identities, DirectoryGroups et DirectoryRoles
```

Cette organisation permet de :

- conserver l’historique des collectes ;
- comparer plusieurs audits d’une même cible ;
- éviter d’écraser les anciennes données ;
- appliquer les mêmes règles CIS aux données Entra ID et Active Directory ;
- produire des résultats propres à chaque audit.

## 2. Identité

Une identité représente un compte provenant de Microsoft Entra ID ou d’Active Directory On-Premise.

| Champ            | Type             | Description                                          |
| ---------------- | ---------------- | ---------------------------------------------------- |
| Id               | UUID             | Identifiant interne de l’application                 |
| AuditId          | UUID             | Audit dans lequel l’identité a été collectée         |
| ExternalId       | Texte            | Identifiant de l’objet dans l’annuaire source        |
| DisplayName      | Texte            | Nom d’affichage                                      |
| UserName         | Texte            | Nom de connexion                                     |
| Email            | Texte nullable   | Adresse électronique                                 |
| Source           | Enum             | `EntraId` ou `ActiveDirectory`                       |
| AccountType      | Enum             | `User`, `Guest`, `Administrator` ou `ServiceAccount` |
| IsEnabled        | Booléen          | Indique si le compte est actif                       |
| IsPrivileged     | Booléen          | Indique si le compte possède des privilèges élevés   |
| IsServiceAccount | Booléen          | Indique s’il s’agit d’un compte de service           |
| IsLocked         | Booléen nullable | Indique si le compte est verrouillé                  |
| LastSignInAt     | Date nullable    | Date de dernière connexion connue                    |
| Description      | Texte nullable   | Description du compte                                |
| Owner            | Texte nullable   | Propriétaire ou responsable du compte                |
| CollectedAt      | Date             | Date et heure de collecte                            |

Une contrainte d’unicité est appliquée sur :

```text
AuditId + ExternalId
```

Elle empêche l’enregistrement plusieurs fois du même objet externe dans un même audit.

## 3. Groupe d’annuaire

Un groupe représente un groupe provenant de Microsoft Entra ID ou d’Active Directory.

| Champ        | Type           | Description                                     |
| ------------ | -------------- | ----------------------------------------------- |
| Id           | UUID           | Identifiant interne                             |
| AuditId      | UUID           | Audit dans lequel le groupe a été collecté      |
| ExternalId   | Texte          | Identifiant du groupe dans l’annuaire source    |
| Name         | Texte          | Nom du groupe                                   |
| Description  | Texte nullable | Description du groupe                           |
| Source       | Enum           | `EntraId` ou `ActiveDirectory`                  |
| GroupType    | Texte nullable | Type du groupe                                  |
| IsPrivileged | Booléen        | Indique si le groupe est sensible ou privilégié |
| CollectedAt  | Date           | Date et heure de collecte                       |

Une contrainte d’unicité est appliquée sur :

```text
AuditId + ExternalId
```

## 4. Appartenance à un groupe

Cette entité représente la relation entre une identité et un groupe d’annuaire.

| Champ          | Type  | Description                           |
| -------------- | ----- | ------------------------------------- |
| Id             | UUID  | Identifiant interne                   |
| IdentityId     | UUID  | Identité membre                       |
| GroupId        | UUID  | Groupe concerné                       |
| MembershipType | Texte | Appartenance `Direct` ou `Transitive` |
| CollectedAt    | Date  | Date et heure de collecte             |

Une contrainte d’unicité est appliquée sur :

```text
IdentityId + GroupId
```

Les identités et les groupes reliés doivent appartenir au même audit.

## 5. Rôle d’annuaire

Un rôle représente une fonction ou un niveau d’autorisation dans Microsoft Entra ID ou Active Directory.

| Champ        | Type           | Description                                   |
| ------------ | -------------- | --------------------------------------------- |
| Id           | UUID           | Identifiant interne                           |
| AuditId      | UUID           | Audit dans lequel le rôle a été collecté      |
| ExternalId   | Texte          | Identifiant du rôle dans l’annuaire source    |
| Name         | Texte          | Nom du rôle                                   |
| Description  | Texte nullable | Description du rôle                           |
| Source       | Enum           | `EntraId` ou `ActiveDirectory`                |
| IsPrivileged | Booléen        | Indique si le rôle est sensible ou privilégié |
| CollectedAt  | Date           | Date et heure de collecte                     |

Une contrainte d’unicité est appliquée sur :

```text
AuditId + ExternalId
```

## 6. Attribution de rôle

Cette entité représente l’attribution d’un rôle d’annuaire à une identité.

| Champ           | Type          | Description                          |
| --------------- | ------------- | ------------------------------------ |
| Id              | UUID          | Identifiant interne                  |
| IdentityId      | UUID          | Identité concernée                   |
| DirectoryRoleId | UUID          | Rôle attribué                        |
| AssignedAt      | Date nullable | Date d’attribution connue            |
| ExpiresAt       | Date nullable | Date d’expiration éventuelle         |
| IsPermanent     | Booléen       | Attribution permanente ou temporaire |
| CollectedAt     | Date          | Date et heure de collecte            |

L’identité et le rôle reliés doivent appartenir au même audit.

## 7. Principes de normalisation

Les données provenant de Microsoft Entra ID et d’Active Directory sont converties vers un modèle commun.

Principes retenus :

- un utilisateur Entra ID et un utilisateur Active Directory sont stockés dans `Identities` ;
- un groupe Entra ID et un groupe Active Directory sont stockés dans `DirectoryGroups` ;
- un rôle Entra ID et un rôle Active Directory sont stockés dans `DirectoryRoles` ;
- chaque objet collecté est associé à un audit précis avec `AuditId` ;
- l’audit est lui-même associé à la cible concernée avec `TargetId` ;
- chaque objet conserve sa source avec `Source` ;
- chaque objet conserve son identifiant d’origine avec `ExternalId` ;
- les différences propres à chaque environnement sont stockées dans des champs optionnels ;
- les champs indisponibles sont enregistrés avec une valeur nulle ou inconnue ;
- les identifiants externes ne sont pas utilisés comme clés primaires ;
- toutes les dates sont enregistrées avec un fuseau horaire ;
- les collecteurs fonctionnent uniquement en lecture sur les annuaires.

## 8. État d’implémentation

Le modèle commun est implémenté pour :

- les cibles ;
- les audits ;
- les identités ;
- les groupes d’annuaire ;
- les appartenances directes et transitives ;
- les rôles d’annuaire ;
- les affectations de rôles ;
- le catalogue des règles CIS ;
- les évaluations ;
- les preuves structurées ;
- les recommandations ;
- les journaux d’audit.

Les collecteurs transmettent les données normalisées à l’API après authentification JWT.

Le collecteur Active Directory a été validé avec un domaine réel de laboratoire en LDAPS sur le port `636`.

Le collecteur Microsoft Entra ID utilise actuellement des fichiers JSON simulant les réponses paginées de Microsoft Graph. Le modèle de données et le cycle d’import sont opérationnels, mais la connexion à un tenant réel reste une évolution.
