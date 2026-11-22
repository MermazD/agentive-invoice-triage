# ADR-0004 : Convertir les erreurs OCR en résultat vide

- Date de rédaction : 2026-10-12
- Statut : Reconstitué à partir de l'implémentation
- Confiance sur le comportement : élevée
- Confiance sur l'intention historique : faible à moyenne

## Contexte

Le plugin journalise les chemins inexistants puis retourne une chaîne vide. Il intercepte également les exceptions de conversion et d'OCR, les journalise, puis retourne une chaîne vide. Son bloc `finally` tente de supprimer les images temporaires. L'agent appelant détecte un texte vide et transforme le résultat en décision `REVIEW`.

Ce comportement paraît adapté à un POC où une facture en échec doit pouvoir être signalée sans arrêter le traitement englobant. La documentation de portée indique que la qualité OCR varie et que les erreurs doivent être signalées, non masquées. Elle ne précise pas pour autant le contrat d'erreur du plugin.

## Décision

Traiter une entrée absente ou un échec d'extraction comme une sortie texte vide, consigner les exceptions avec le logger, et laisser l'orchestrateur traduire l'absence de texte en décision de revue. Tenter le nettoyage des images temporaires sans faire échouer le flux si leur suppression échoue.

## Options considérées

- Propager une exception typée à l'orchestrateur : rend l'échec explicite et différenciable, mais exige une politique de reprise et de gestion d'erreur à chaque appelant.
- Retourner un résultat discriminé (`Success`, texte, erreur) : rend le contrat explicite sans exception de contrôle, au prix d'un changement de modèle de résultat.
- Réessayer automatiquement : peut aider face à des échecs transitoires, mais ne corrige pas les erreurs permanentes de binaire, de modèle linguistique ou de document.

Ces alternatives n'ont pas de trace de décision dans les sources disponibles.

## Conséquences

- L'échec OCR ne fait pas nécessairement tomber le triage; l'agent peut orienter la facture vers une revue manuelle.
- Une chaîne vide ne distingue pas un PDF vide, un échec du rendu, un échec Tesseract ou une sortie OCR sans texte. L'orchestrateur dispose de peu d'informations pour décider ou mesurer ces cas.
- Les exceptions sont conservées dans les journaux, mais l'appelant ne reçoit pas leur type ni leur cause.
- Dans `RunTesseractAsync`, la sortie stderr est redirigée mais non lue, le code de sortie n'est pas vérifié et le contenu stdout est retourné même en cas d'échec du processus. Cela peut transformer un échec natif en résultat partiel ou vide sans diagnostic remonté.
- Ignorer les erreurs de suppression favorise la continuité, mais peut laisser des fichiers temporaires derrière.
