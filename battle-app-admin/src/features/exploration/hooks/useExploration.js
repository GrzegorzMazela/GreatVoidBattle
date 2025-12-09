import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { explorationApi } from '../../../services/explorationApi';

// Query keys
export const explorationKeys = {
  all: ['exploration'],
  systems: () => [...explorationKeys.all, 'systems'],
  system: (id) => [...explorationKeys.systems(), id],
  
  fraction: (fractionId) => [...explorationKeys.all, 'fraction', fractionId],
  knownSystems: (fractionId) => [...explorationKeys.fraction(fractionId), 'known'],
  status: (fractionId) => [...explorationKeys.fraction(fractionId), 'status'],
  expeditions: (fractionId) => [...explorationKeys.fraction(fractionId), 'expeditions'],
  history: (fractionId) => [...explorationKeys.fraction(fractionId), 'history'],
  researchSlots: (fractionId) => [...explorationKeys.fraction(fractionId), 'slots'],
  
  admin: () => [...explorationKeys.all, 'admin'],
  allExpeditions: () => [...explorationKeys.admin(), 'expeditions'],
  allSlots: () => [...explorationKeys.admin(), 'slots'],
  systemNotes: (systemId) => [...explorationKeys.admin(), 'notes', systemId],
  discoveryStatus: (systemId) => [...explorationKeys.admin(), 'discovery', systemId],
};

// ==================== UKŁADY GWIEZDNE ====================

/**
 * Hook do pobierania wszystkich układów
 */
export function useAllSystems() {
  return useQuery({
    queryKey: explorationKeys.systems(),
    queryFn: explorationApi.getAllSystems,
    staleTime: 5 * 60 * 1000, // 5 minut
  });
}

/**
 * Hook do pobierania szczegółów układu
 */
export function useSystem(systemId) {
  return useQuery({
    queryKey: explorationKeys.system(systemId),
    queryFn: () => explorationApi.getSystem(systemId),
    enabled: !!systemId,
  });
}

/**
 * Hook do aktualizacji układu (admin)
 */
export function useUpdateSystem() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ systemId, data }) => explorationApi.updateSystem(systemId, data),
    onSuccess: (_, { systemId }) => {
      queryClient.invalidateQueries({ queryKey: explorationKeys.system(systemId) });
      queryClient.invalidateQueries({ queryKey: explorationKeys.systems() });
    },
  });
}

// ==================== EKSPLORACJA FRAKCJI ====================

/**
 * Hook do pobierania znanych układów frakcji
 */
export function useKnownSystems(fractionId) {
  return useQuery({
    queryKey: explorationKeys.knownSystems(fractionId),
    queryFn: () => explorationApi.getKnownSystems(fractionId),
    enabled: !!fractionId,
  });
}

/**
 * Hook do pobierania statusu eksploracji frakcji
 */
export function useExplorationStatus(fractionId) {
  return useQuery({
    queryKey: explorationKeys.status(fractionId),
    queryFn: () => explorationApi.getExplorationStatus(fractionId),
    enabled: !!fractionId,
  });
}

/**
 * Hook do wysyłania ekspedycji
 */
export function useSendExpedition(fractionId) {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (systemId) => explorationApi.sendExpedition(fractionId, systemId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: explorationKeys.expeditions(fractionId) });
      queryClient.invalidateQueries({ queryKey: explorationKeys.status(fractionId) });
      queryClient.invalidateQueries({ queryKey: explorationKeys.allExpeditions() });
    },
  });
}

/**
 * Hook do anulowania ekspedycji
 */
export function useCancelExpedition(fractionId) {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (expeditionId) => explorationApi.cancelExpedition(fractionId, expeditionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: explorationKeys.expeditions(fractionId) });
      queryClient.invalidateQueries({ queryKey: explorationKeys.status(fractionId) });
      queryClient.invalidateQueries({ queryKey: explorationKeys.allExpeditions() });
    },
  });
}

/**
 * Hook do pobierania aktywnych ekspedycji frakcji
 */
export function usePendingExpeditions(fractionId) {
  return useQuery({
    queryKey: explorationKeys.expeditions(fractionId),
    queryFn: () => explorationApi.getPendingExpeditions(fractionId),
    enabled: !!fractionId,
    refetchInterval: 30 * 1000, // Odświeżaj co 30 sekund
  });
}

/**
 * Hook do pobierania historii odkryć
 */
export function useExplorationHistory(fractionId) {
  return useQuery({
    queryKey: explorationKeys.history(fractionId),
    queryFn: () => explorationApi.getExplorationHistory(fractionId),
    enabled: !!fractionId,
  });
}

/**
 * Hook do pobierania slotów badawczych frakcji
 */
export function useResearchSlots(fractionId) {
  return useQuery({
    queryKey: explorationKeys.researchSlots(fractionId),
    queryFn: () => explorationApi.getResearchSlots(fractionId),
    enabled: !!fractionId,
  });
}

// ==================== ADMIN ====================

/**
 * Hook do pobierania wszystkich aktywnych ekspedycji (admin)
 */
export function useAllPendingExpeditions() {
  return useQuery({
    queryKey: explorationKeys.allExpeditions(),
    queryFn: explorationApi.getAllPendingExpeditions,
    refetchInterval: 30 * 1000, // Odświeżaj co 30 sekund
  });
}

/**
 * Hook do rozpatrywania ekspedycji (admin)
 */
export function useResolveExpedition() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ expeditionId, resolution }) => 
      explorationApi.resolveExpedition(expeditionId, resolution),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: explorationKeys.allExpeditions() });
      queryClient.invalidateQueries({ queryKey: explorationKeys.all });
    },
  });
}

/**
 * Hook do odrzucania ekspedycji (admin)
 */
export function useRejectExpedition() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ expeditionId, reason }) => 
      explorationApi.rejectExpedition(expeditionId, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: explorationKeys.allExpeditions() });
    },
  });
}

/**
 * Hook do wysyłania informacji o układzie (admin)
 */
export function useSendSystemInfo() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ systemId, fractionId, data }) => 
      explorationApi.sendSystemInfo(systemId, fractionId, data),
    onSuccess: (_, { systemId }) => {
      queryClient.invalidateQueries({ queryKey: explorationKeys.discoveryStatus(systemId) });
      queryClient.invalidateQueries({ queryKey: explorationKeys.all });
    },
  });
}

/**
 * Hook do pobierania notatek o układzie (admin)
 */
export function useSystemNotes(systemId) {
  return useQuery({
    queryKey: explorationKeys.systemNotes(systemId),
    queryFn: () => explorationApi.getSystemNotes(systemId),
    enabled: !!systemId,
  });
}

/**
 * Hook do zapisywania notatek o układzie (admin)
 */
export function useSaveSystemNotes() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ systemId, notes }) => explorationApi.saveSystemNotes(systemId, notes),
    onSuccess: (_, { systemId }) => {
      queryClient.invalidateQueries({ queryKey: explorationKeys.systemNotes(systemId) });
    },
  });
}

/**
 * Hook do pobierania statusu odkrycia układu (admin)
 */
export function useDiscoveryStatus(systemId) {
  return useQuery({
    queryKey: explorationKeys.discoveryStatus(systemId),
    queryFn: () => explorationApi.getSystemDiscoveryStatus(systemId),
    enabled: !!systemId,
  });
}

/**
 * Hook do pobierania wszystkich slotów badawczych (admin)
 */
export function useAllResearchSlots() {
  return useQuery({
    queryKey: explorationKeys.allSlots(),
    queryFn: explorationApi.getAllResearchSlots,
  });
}

/**
 * Hook do ustawiania slotów badawczych (admin)
 */
export function useSetResearchSlots() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ fractionId, slots }) => explorationApi.setResearchSlots(fractionId, slots),
    onSuccess: (_, { fractionId }) => {
      queryClient.invalidateQueries({ queryKey: explorationKeys.researchSlots(fractionId) });
      queryClient.invalidateQueries({ queryKey: explorationKeys.allSlots() });
    },
  });
}

/**
 * Hook do zakończenia tury eksploracji (admin)
 */
export function useEndExplorationTurn() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: explorationApi.endExplorationTurn,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: explorationKeys.all });
    },
  });
}

export default {
  explorationKeys,
  useAllSystems,
  useSystem,
  useUpdateSystem,
  useKnownSystems,
  useExplorationStatus,
  useSendExpedition,
  useCancelExpedition,
  usePendingExpeditions,
  useExplorationHistory,
  useResearchSlots,
  useAllPendingExpeditions,
  useResolveExpedition,
  useRejectExpedition,
  useSendSystemInfo,
  useSystemNotes,
  useSaveSystemNotes,
  useDiscoveryStatus,
  useAllResearchSlots,
  useSetResearchSlots,
  useEndExplorationTurn,
};

