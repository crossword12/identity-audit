# Catalogue initial des règles d’audit

## Principes généraux

Chaque règle possède :

- un identifiant unique ;
- un nom ;
- une description ;
- un environnement concerné ;
- un contrôle CIS associé ;
- une condition d’évaluation ;
- une gravité ;
- une recommandation ;
- un état actif ou inactif.

Les résultats possibles sont :

- `Compliant` : règle respectée ;
- `NonCompliant` : anomalie détectée ;
- `NotApplicable` : règle non applicable ;
- `NotVerifiable` : données ou permissions insuffisantes.

---

## AD-01 — Compte dormant actif

| Élément        | Valeur                                                                      |
| -------------- | --------------------------------------------------------------------------- |
| Cible          | Active Directory                                                            |
| CIS            | 5.3                                                                         |
| Gravité        | Élevée                                                                      |
| Condition      | Le compte est actif et sa dernière connexion dépasse une durée configurable |
| Seuil initial  | 90 jours                                                                    |
| Preuve         | Date de dernière connexion et état du compte                                |
| Recommandation | Vérifier la nécessité du compte et le désactiver s’il n’est plus utilisé    |

---

## AD-02 — Compte administrateur non dédié

| Élément        | Valeur                                                                           |
| -------------- | -------------------------------------------------------------------------------- |
| Cible          | Active Directory                                                                 |
| CIS            | 5.4                                                                              |
| Gravité        | Élevée                                                                           |
| Condition      | Un compte privilégié semble également utilisé comme compte utilisateur quotidien |
| Preuve         | Appartenance à un groupe privilégié et activité régulière du compte              |
| Recommandation | Séparer le compte standard du compte d’administration                            |

Cette règle pourra être marquée `NotVerifiable` lorsque les données collectées ne permettent pas de confirmer l’usage quotidien du compte.

---

## AD-03 — Compte de service non documenté

| Élément        | Valeur                                                                                         |
| -------------- | ---------------------------------------------------------------------------------------------- |
| Cible          | Active Directory                                                                               |
| CIS            | 5.5                                                                                            |
| Gravité        | Moyenne                                                                                        |
| Condition      | Le compte est identifié comme compte de service mais ne possède ni description ni propriétaire |
| Preuve         | Type du compte, description et propriétaire                                                    |
| Recommandation | Documenter la fonction du compte et désigner un responsable                                    |

---

## AD-04 — Compte désactivé encore privilégié

| Élément        | Valeur                                                           |
| -------------- | ---------------------------------------------------------------- |
| Cible          | Active Directory                                                 |
| CIS            | 6.2                                                              |
| Gravité        | Élevée                                                           |
| Condition      | Le compte est désactivé mais reste membre d’un groupe privilégié |
| Preuve         | État du compte et liste des groupes privilégiés                  |
| Recommandation | Retirer les appartenances privilégiées devenues inutiles         |

---

## AD-05 — Appartenance à un groupe privilégié

| Élément        | Valeur                                                                                      |
| -------------- | ------------------------------------------------------------------------------------------- |
| Cible          | Active Directory                                                                            |
| CIS            | 6.8                                                                                         |
| Gravité        | Élevée                                                                                      |
| Condition      | Un utilisateur est membre d’un groupe sensible comme Domain Admins                          |
| Preuve         | Nom du compte et nom du groupe privilégié                                                   |
| Recommandation | Vérifier que cette appartenance est justifiée et appliquer le principe du moindre privilège |

---

## ENTRA-01 — Configuration MFA non confirmée

| Élément        | Valeur                                                                                           |
| -------------- | ------------------------------------------------------------------------------------------------ |
| Cible          | Microsoft Entra ID                                                                               |
| CIS            | 6.5                                                                                              |
| Gravité        | Critique                                                                                         |
| Condition      | Aucune méthode MFA ou information d’application du MFA n’est confirmée pour un compte privilégié |
| Preuve         | Rôles du compte et données MFA accessibles                                                       |
| Recommandation | Activer et imposer une authentification multifacteur pour le compte privilégié                   |

Lorsque les permissions Microsoft Graph ne permettent pas de vérifier le MFA, le résultat sera `NotVerifiable`.

---

## ENTRA-02 — Invité inactif

| Élément        | Valeur                                                                              |
| -------------- | ----------------------------------------------------------------------------------- |
| Cible          | Microsoft Entra ID                                                                  |
| CIS            | 5.3                                                                                 |
| Gravité        | Moyenne                                                                             |
| Condition      | Le compte est de type invité et sa dernière activité dépasse une durée configurable |
| Seuil initial  | 90 jours                                                                            |
| Preuve         | Type du compte et date de dernière connexion                                        |
| Recommandation | Vérifier si l’accès externe est toujours nécessaire et supprimer les accès inutiles |

---

## ENTRA-03 — Compte désactivé possédant encore un rôle

| Élément        | Valeur                                                         |
| -------------- | -------------------------------------------------------------- |
| Cible          | Microsoft Entra ID                                             |
| CIS            | 6.2                                                            |
| Gravité        | Élevée                                                         |
| Condition      | Le compte est désactivé mais possède encore un rôle d’annuaire |
| Preuve         | État du compte et rôle attribué                                |
| Recommandation | Révoquer les rôles qui ne sont plus nécessaires                |

---

## Résumé

| ID       | Règle                                     | Cible            | CIS | Gravité  |
| -------- | ----------------------------------------- | ---------------- | --- | -------- |
| AD-01    | Compte dormant actif                      | Active Directory | 5.3 | Élevée   |
| AD-02    | Compte administrateur non dédié           | Active Directory | 5.4 | Élevée   |
| AD-03    | Compte de service non documenté           | Active Directory | 5.5 | Moyenne  |
| AD-04    | Compte désactivé encore privilégié        | Active Directory | 6.2 | Élevée   |
| AD-05    | Appartenance à un groupe privilégié       | Active Directory | 6.8 | Élevée   |
| ENTRA-01 | Configuration MFA non confirmée           | Entra ID         | 6.5 | Critique |
| ENTRA-02 | Invité inactif                            | Entra ID         | 5.3 | Moyenne  |
| ENTRA-03 | Compte désactivé possédant encore un rôle | Entra ID         | 6.2 | Élevée   |
