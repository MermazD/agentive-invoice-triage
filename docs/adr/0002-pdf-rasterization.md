# ADR-0002 : Rasteriser les PDF avant l'OCR

- Date de rédaction : 2026-10-12
- Statut : Reconstitué à partir de l'implémentation
- Confiance sur le choix : élevée
- Confiance sur les motivations historiques : moyenne

## Contexte

Le plugin ouvre le PDF avec un flux, le convertit en images avec `PDFtoImage`, encode chaque page en PNG avec SkiaSharp, puis transmet le chemin de cette image au processus Tesseract. Les pages sont traitées séquentiellement. Le texte non vide est concaténé avec un séparateur de page.

Une motivation vraisemblable est de prendre en charge les factures scannées via un moteur OCR d'images, tout en gardant un pipeline de POC simple. Le code ne documente pas le choix de résolution, la taille maximale de document ni un objectif de débit.

## Décision

Convertir toutes les pages du PDF en images, écrire chaque image PNG dans le répertoire temporaire du système, exécuter Tesseract page par page, puis agréger les sorties dans une seule chaîne. Supprimer les fichiers temporaires dans un bloc `finally`.

## Options considérées

- Extraction directe du texte natif du PDF, avec OCR en repli : plus rapide pour les PDF textuels, mais exige un chemin hybride et la gestion de la mise en page.
- Transmission en mémoire à une API OCR : évite les images temporaires, mais nécessite une intégration OCR compatible avec les données d'image en mémoire.
- Traitement parallèle des pages : pourrait réduire la latence, mais complexifie la maîtrise de la consommation mémoire et du nombre de processus.
- Un format temporaire différent : possible, mais PNG donne une image sans perte et se transmet simplement au CLI.

Ce sont des alternatives déduites; aucune comparaison explicite n'est présente dans les sources.

## Conséquences

- Le chemin accepte le format PDF même lorsque ses pages sont des scans.
- La frontière fichier-image convient à l'exécutable Tesseract et rend chaque étape relativement simple à observer.
- Le PDF et toutes ses pages rendues sont chargés avant la boucle OCR (`ToList`), ce qui augmente le pic mémoire pour les documents volumineux.
- Le travail séquentiel borne naturellement le nombre de processus simultanés, au prix d'une latence proportionnelle au nombre de pages.
- Les PNG temporaires consomment de l'espace disque; la suppression est tentée à la fin, mais les erreurs de suppression sont ignorées.
- Le texte agrégé conserve seulement un marqueur de page; les coordonnées et la structure visuelle ne sont pas transmises au composant suivant.
