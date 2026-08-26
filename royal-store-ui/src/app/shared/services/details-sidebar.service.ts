import { Injectable, signal, Type } from '@angular/core';

// Define a estrutura do conteúdo dinâmico
export interface DrawerContent {
  component: Type<any>; // O tipo de classe do componente (ex: PetDetailsContentComponent)
  props?: DrawerProps; // Os dados que serão passados para o componente injetado
}

export interface DrawerProps {
  title?: string;
  subtitle?: string;
}

@Injectable({
  providedIn: 'root',
})
export class DetailsSidebarService {
  contentToRender = signal<DrawerContent | null>(null);

  // Sinal para controlar a visibilidade da "casca" do drawer
  sidebarVisible = signal(false);

  /**
   * Abre o drawer injetando um componente
   * @param component O componente (a classe) a ser carregado.
   * @param data Os dados (o registro) a ser exibido.
   */
  openDrawer(component: Type<any>, data?: DrawerProps) {
    this.contentToRender.set({ component: component, props: { ...data } });
    this.sidebarVisible.set(true);
  }

  closeDrawer() {
    this.sidebarVisible.set(false);
    // O conteúdo será limpo pelo efeito no componente shell após o fechamento
  }

  navigate() {
    this.sidebarVisible.set(true);
  }
}
