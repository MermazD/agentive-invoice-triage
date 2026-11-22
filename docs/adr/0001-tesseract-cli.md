# ADR-0001 : Exécuter Tesseract via son interface en ligne de commande

- Date de rédaction : 2026-10-12
- Statut : Reconstitué à partir de l'implémentation
- Confiance sur le choix : élevée
- Confiance sur les motivations historiques : moyenne à faible

## Contexte

`InvoiceExtractionPlugin` lance un processus externe nommé `tesseract`, lui transmet une image et récupère le texte sur la sortie standard. Le benchmark OCR du dépôt utilise également Tesseract. La documentation de benchmark indique une installation locale de Tesseract comme prérequis.

Le dépôt ne fournit pas d'historique Git exploitable ici pour confirmer les discussions initiales. Le contexte ci-dessous est donc une reconstruction : il s'agissait vraisemblablement d'un POC visant des factures image/scannées, avec une solution OCR locale réutilisable et mesurable.

## Décision

Utiliser le binaire Tesseract installé sur la machine d'exécution, invoqué comme processus enfant pour chaque image rendue. Les langues configurées sont le français et l'anglais (`fra+eng`); le moteur est lancé avec `--oem 3 --psm 6`.

## Options considérées

- Une bibliothèque .NET intégrant Tesseract : aurait évité de gérer un processus externe, mais ajouté une dépendance native et une intégration spécifique au runtime.
- Un service OCR cloud : aurait délégué l'exploitation du moteur et pu fournir des résultats plus structurés, mais ajouté réseau, identité, coût et transfert de données.
- Extraire uniquement le texte PDF natif : rapide pour les PDF numériques, mais insuffisant pour les documents scannés ou contenant des pages image.

Ces options sont des alternatives plausibles, pas des options dont le rejet est attesté par les sources présentes.

## Conséquences

- Le texte est traité localement et le même moteur peut être évalué par le benchmark du dépôt.
- L'environnement doit installer Tesseract, fournir les modèles linguistiques `fra` et `eng`, et rendre l'exécutable accessible via `PATH`.
- L'application dépend du démarrage et du code de sortie d'un processus natif; disponibilité, permissions, version du moteur et erreurs stderr sont des risques d'exploitation.
- Chaque page entraîne un lancement de processus distinct, ce qui peut augmenter la latence sur les documents longs.
- Les réglages linguistiques et de segmentation sont fixés dans le code et ne sont pas configurables par appelant.
