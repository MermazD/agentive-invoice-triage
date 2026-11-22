# ADR-0005 : Ne pas persister les données extraites dans le module OCR

- Date de rédaction : 2026-10-12
- Statut : Inférence à confirmer
- Confiance sur l'absence de persistance dans ce module : élevée
- Confiance sur l'intention de conception : faible

## Contexte

Le plugin lit un fichier fourni par chemin, écrit des images intermédiaires dans le dossier temporaire, retourne le texte à l'orchestrateur et tente de supprimer les images. Aucune base de données, file de messages, table d'audit ou écriture durable du texte n'apparaît dans ce module. La documentation de portée décrit un POC interne, une rétention limitée conforme au cadre RGPD et exclut un usage comptable de production.

Il s'agit d'une décision reconstruite à partir de l'absence de persistance et du périmètre POC; aucune source ne confirme qu'une base de données a été évaluée puis explicitement rejetée. Aucun choix de SGBD ne peut donc être attribué aux développeurs à partir du code fourni.

## Décision

Pour le périmètre actuel du plugin, ne pas ajouter de stockage persistant dédié aux PDF, images rendues ou textes OCR. Utiliser uniquement les flux en mémoire et les images temporaires nécessaires au traitement, avec tentative de suppression des intermédiaires.

## Options considérées

- Stocker les fichiers et résultats dans un stockage objet : facilite la reprise et l'audit, mais impose des règles de rétention, de contrôle d'accès et de suppression.
- Persister les métadonnées/résultats dans une base relationnelle ou documentaire : permet le suivi des décisions et des recherches, mais ajoute un schéma, des migrations, des opérations et une responsabilité de conformité.
- Envoyer les documents vers un service OCR externe : peut déléguer le calcul et offrir un historique de tâches, mais change le périmètre de traitement et le transfert de données.

Ces options sont des alternatives architecturales, pas des propositions attestées comme ayant été examinées à l'époque.

## Conséquences

- Le module n'introduit pas de dépendance à un SGBD ni à un stockage durable et garde le POC plus simple à exécuter.
- Il n'existe pas dans ce module de reprise après interruption, de rejeu, de piste d'audit durable ou de comparaison historique des résultats.
- La facture source reste sur son emplacement d'origine; l'absence de stockage OCR ne garantit donc pas à elle seule une durée de conservation limitée.
- Les fichiers temporaires contiennent des données potentiellement sensibles pendant le traitement et leur suppression n'est ni une suppression sécurisée ni une garantie contre une panne de processus.
- Toute mise en production nécessiterait de définir explicitement les exigences de conservation, d'audit, d'accès et d'effacement avant d'ajouter une persistance.
