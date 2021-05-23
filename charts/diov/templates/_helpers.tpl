{{/* vim: set filetype=mustache: */}}
{{/*
Expand the name of the chart.
*/}}
{{- define "diov.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "diov.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "diov.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
DB labels
*/}}
{{- define "diov.db.labels" -}}
helm.sh/chart: {{ include "diov.chart" . }}
{{ include "diov.db.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
DB selector labels
*/}}
{{- define "diov.db.selectorLabels" -}}
app.kubernetes.io/name: {{ include "diov.name" . }}-db
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Redis labels
*/}}
{{- define "diov.redis.labels" -}}
helm.sh/chart: {{ include "diov.chart" . }}
{{ include "diov.redis.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Redis selector labels
*/}}
{{- define "diov.redis.selectorLabels" -}}
app.kubernetes.io/name: {{ include "diov.name" . }}-redis
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Web labels
*/}}
{{- define "diov.web.labels" -}}
helm.sh/chart: {{ include "diov.chart" . }}
{{ include "diov.web.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Web selector labels
*/}}
{{- define "diov.web.selectorLabels" -}}
app.kubernetes.io/name: {{ include "diov.name" . }}-web
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}
