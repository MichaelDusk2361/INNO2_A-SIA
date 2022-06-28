'use strict';

customElements.define('compodoc-menu', class extends HTMLElement {
    constructor() {
        super();
        this.isNormalMode = this.getAttribute('mode') === 'normal';
    }

    connectedCallback() {
        this.render(this.isNormalMode);
    }

    render(isNormalMode) {
        let tp = lithtml.html(`
        <nav>
            <ul class="list">
                <li class="title">
                    <a href="index.html" data-type="index-link">a-sia documentation</a>
                </li>

                <li class="divider"></li>
                ${ isNormalMode ? `<div id="book-search-input" role="search"><input type="text" placeholder="Type to search"></div>` : '' }
                <li class="chapter">
                    <a data-type="chapter-link" href="index.html"><span class="icon ion-ios-home"></span>Getting started</a>
                    <ul class="links">
                        <li class="link">
                            <a href="overview.html" data-type="chapter-link">
                                <span class="icon ion-ios-keypad"></span>Overview
                            </a>
                        </li>
                        <li class="link">
                            <a href="index.html" data-type="chapter-link">
                                <span class="icon ion-ios-paper"></span>README
                            </a>
                        </li>
                                <li class="link">
                                    <a href="dependencies.html" data-type="chapter-link">
                                        <span class="icon ion-ios-list"></span>Dependencies
                                    </a>
                                </li>
                    </ul>
                </li>
                    <li class="chapter modules">
                        <a data-type="chapter-link" href="modules.html">
                            <div class="menu-toggler linked" data-toggle="collapse" ${ isNormalMode ?
                                'data-target="#modules-links"' : 'data-target="#xs-modules-links"' }>
                                <span class="icon ion-ios-archive"></span>
                                <span class="link-name">Modules</span>
                                <span class="icon ion-ios-arrow-down"></span>
                            </div>
                        </a>
                        <ul class="links collapse " ${ isNormalMode ? 'id="modules-links"' : 'id="xs-modules-links"' }>
                            <li class="link">
                                <a href="modules/AppModule.html" data-type="entity-link" >AppModule</a>
                                    <li class="chapter inner">
                                        <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ?
                                            'data-target="#components-links-module-AppModule-55152d09f8b926bac9395f88afdbe02c"' : 'data-target="#xs-components-links-module-AppModule-55152d09f8b926bac9395f88afdbe02c"' }>
                                            <span class="icon ion-md-cog"></span>
                                            <span>Components</span>
                                            <span class="icon ion-ios-arrow-down"></span>
                                        </div>
                                        <ul class="links collapse" ${ isNormalMode ? 'id="components-links-module-AppModule-55152d09f8b926bac9395f88afdbe02c"' :
                                            'id="xs-components-links-module-AppModule-55152d09f8b926bac9395f88afdbe02c"' }>
                                            <li class="link">
                                                <a href="components/AppComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >AppComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/LoginComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >LoginComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/NetworkViewComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >NetworkViewComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/SettingsComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >SettingsComponent</a>
                                            </li>
                                        </ul>
                                    </li>
                            </li>
                            <li class="link">
                                <a href="modules/AppRoutingModule.html" data-type="entity-link" >AppRoutingModule</a>
                            </li>
                            <li class="link">
                                <a href="modules/DataPanelModule.html" data-type="entity-link" >DataPanelModule</a>
                                    <li class="chapter inner">
                                        <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ?
                                            'data-target="#components-links-module-DataPanelModule-529cbee843865aec889aec4216147623"' : 'data-target="#xs-components-links-module-DataPanelModule-529cbee843865aec889aec4216147623"' }>
                                            <span class="icon ion-md-cog"></span>
                                            <span>Components</span>
                                            <span class="icon ion-ios-arrow-down"></span>
                                        </div>
                                        <ul class="links collapse" ${ isNormalMode ? 'id="components-links-module-DataPanelModule-529cbee843865aec889aec4216147623"' :
                                            'id="xs-components-links-module-DataPanelModule-529cbee843865aec889aec4216147623"' }>
                                            <li class="link">
                                                <a href="components/DataPanelComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >DataPanelComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/DataPanelNavbarComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >DataPanelNavbarComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/DataPanelNavbarEntryComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >DataPanelNavbarEntryComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/InspectorPanelComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >InspectorPanelComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/NetworkPanelComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >NetworkPanelComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/StructurePanelComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >StructurePanelComponent</a>
                                            </li>
                                        </ul>
                                    </li>
                            </li>
                            <li class="link">
                                <a href="modules/GraphEditorModule.html" data-type="entity-link" >GraphEditorModule</a>
                                    <li class="chapter inner">
                                        <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ?
                                            'data-target="#components-links-module-GraphEditorModule-c176e8067ccb34755d619d31dab71ec2"' : 'data-target="#xs-components-links-module-GraphEditorModule-c176e8067ccb34755d619d31dab71ec2"' }>
                                            <span class="icon ion-md-cog"></span>
                                            <span>Components</span>
                                            <span class="icon ion-ios-arrow-down"></span>
                                        </div>
                                        <ul class="links collapse" ${ isNormalMode ? 'id="components-links-module-GraphEditorModule-c176e8067ccb34755d619d31dab71ec2"' :
                                            'id="xs-components-links-module-GraphEditorModule-c176e8067ccb34755d619d31dab71ec2"' }>
                                            <li class="link">
                                                <a href="components/ConnectionComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >ConnectionComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/GraphControlSimulationNavigationComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >GraphControlSimulationNavigationComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/GraphControlToolComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >GraphControlToolComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/GraphControlZoomComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >GraphControlZoomComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/GraphEditorComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >GraphEditorComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/GraphEditorControlsComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >GraphEditorControlsComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/GroupNodeComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >GroupNodeComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/PersonNodeComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >PersonNodeComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/SocialNodeComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >SocialNodeComponent</a>
                                            </li>
                                        </ul>
                                    </li>
                            </li>
                            <li class="link">
                                <a href="modules/MenubarModule.html" data-type="entity-link" >MenubarModule</a>
                                    <li class="chapter inner">
                                        <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ?
                                            'data-target="#components-links-module-MenubarModule-8353059721bdccf07756a9c8d89364c2"' : 'data-target="#xs-components-links-module-MenubarModule-8353059721bdccf07756a9c8d89364c2"' }>
                                            <span class="icon ion-md-cog"></span>
                                            <span>Components</span>
                                            <span class="icon ion-ios-arrow-down"></span>
                                        </div>
                                        <ul class="links collapse" ${ isNormalMode ? 'id="components-links-module-MenubarModule-8353059721bdccf07756a9c8d89364c2"' :
                                            'id="xs-components-links-module-MenubarModule-8353059721bdccf07756a9c8d89364c2"' }>
                                            <li class="link">
                                                <a href="components/MenubarComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >MenubarComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/MenubarEntryComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >MenubarEntryComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/MenubarMenuComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >MenubarMenuComponent</a>
                                            </li>
                                        </ul>
                                    </li>
                            </li>
                            <li class="link">
                                <a href="modules/NetworkExplorerModule.html" data-type="entity-link" >NetworkExplorerModule</a>
                                    <li class="chapter inner">
                                        <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ?
                                            'data-target="#components-links-module-NetworkExplorerModule-5daedcf4baadebe050cfec5d1c7a9d01"' : 'data-target="#xs-components-links-module-NetworkExplorerModule-5daedcf4baadebe050cfec5d1c7a9d01"' }>
                                            <span class="icon ion-md-cog"></span>
                                            <span>Components</span>
                                            <span class="icon ion-ios-arrow-down"></span>
                                        </div>
                                        <ul class="links collapse" ${ isNormalMode ? 'id="components-links-module-NetworkExplorerModule-5daedcf4baadebe050cfec5d1c7a9d01"' :
                                            'id="xs-components-links-module-NetworkExplorerModule-5daedcf4baadebe050cfec5d1c7a9d01"' }>
                                            <li class="link">
                                                <a href="components/NetworkExplorerComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >NetworkExplorerComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/NetworkExplorerInstancesComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >NetworkExplorerInstancesComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/NetworkExplorerNetworkComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >NetworkExplorerNetworkComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/NetworkExplorerNetworksComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >NetworkExplorerNetworksComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/NetworkExplorerProjectsComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >NetworkExplorerProjectsComponent</a>
                                            </li>
                                        </ul>
                                    </li>
                            </li>
                            <li class="link">
                                <a href="modules/PaneModule.html" data-type="entity-link" >PaneModule</a>
                                    <li class="chapter inner">
                                        <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ?
                                            'data-target="#components-links-module-PaneModule-a558c3431f72ecb8dcfedf1f7c9b8318"' : 'data-target="#xs-components-links-module-PaneModule-a558c3431f72ecb8dcfedf1f7c9b8318"' }>
                                            <span class="icon ion-md-cog"></span>
                                            <span>Components</span>
                                            <span class="icon ion-ios-arrow-down"></span>
                                        </div>
                                        <ul class="links collapse" ${ isNormalMode ? 'id="components-links-module-PaneModule-a558c3431f72ecb8dcfedf1f7c9b8318"' :
                                            'id="xs-components-links-module-PaneModule-a558c3431f72ecb8dcfedf1f7c9b8318"' }>
                                            <li class="link">
                                                <a href="components/PaneComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >PaneComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/PaneContainerComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >PaneContainerComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/PaneDividerComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >PaneDividerComponent</a>
                                            </li>
                                        </ul>
                                    </li>
                                <li class="chapter inner">
                                    <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ?
                                        'data-target="#directives-links-module-PaneModule-a558c3431f72ecb8dcfedf1f7c9b8318"' : 'data-target="#xs-directives-links-module-PaneModule-a558c3431f72ecb8dcfedf1f7c9b8318"' }>
                                        <span class="icon ion-md-code-working"></span>
                                        <span>Directives</span>
                                        <span class="icon ion-ios-arrow-down"></span>
                                    </div>
                                    <ul class="links collapse" ${ isNormalMode ? 'id="directives-links-module-PaneModule-a558c3431f72ecb8dcfedf1f7c9b8318"' :
                                        'id="xs-directives-links-module-PaneModule-a558c3431f72ecb8dcfedf1f7c9b8318"' }>
                                        <li class="link">
                                            <a href="directives/PaneIfDirective.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >PaneIfDirective</a>
                                        </li>
                                    </ul>
                                </li>
                            </li>
                            <li class="link">
                                <a href="modules/SharedModule.html" data-type="entity-link" >SharedModule</a>
                                    <li class="chapter inner">
                                        <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ?
                                            'data-target="#components-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' : 'data-target="#xs-components-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' }>
                                            <span class="icon ion-md-cog"></span>
                                            <span>Components</span>
                                            <span class="icon ion-ios-arrow-down"></span>
                                        </div>
                                        <ul class="links collapse" ${ isNormalMode ? 'id="components-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' :
                                            'id="xs-components-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' }>
                                            <li class="link">
                                                <a href="components/ColorInputComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >ColorInputComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/DropDownComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >DropDownComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/InputLargeComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >InputLargeComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/InputMediumComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >InputMediumComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/InputSmallComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >InputSmallComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/ModalComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >ModalComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/SliderComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >SliderComponent</a>
                                            </li>
                                        </ul>
                                    </li>
                                <li class="chapter inner">
                                    <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ?
                                        'data-target="#directives-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' : 'data-target="#xs-directives-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' }>
                                        <span class="icon ion-md-code-working"></span>
                                        <span>Directives</span>
                                        <span class="icon ion-ios-arrow-down"></span>
                                    </div>
                                    <ul class="links collapse" ${ isNormalMode ? 'id="directives-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' :
                                        'id="xs-directives-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' }>
                                        <li class="link">
                                            <a href="directives/ButtonDirective.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >ButtonDirective</a>
                                        </li>
                                        <li class="link">
                                            <a href="directives/ButtonSmallDirective.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >ButtonSmallDirective</a>
                                        </li>
                                        <li class="link">
                                            <a href="directives/InputLargeDirective.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >InputLargeDirective</a>
                                        </li>
                                        <li class="link">
                                            <a href="directives/InputMediumDirective.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >InputMediumDirective</a>
                                        </li>
                                        <li class="link">
                                            <a href="directives/InputSmallDirective.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >InputSmallDirective</a>
                                        </li>
                                    </ul>
                                </li>
                                <li class="chapter inner">
                                    <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ?
                                        'data-target="#injectables-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' : 'data-target="#xs-injectables-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' }>
                                        <span class="icon ion-md-arrow-round-down"></span>
                                        <span>Injectables</span>
                                        <span class="icon ion-ios-arrow-down"></span>
                                    </div>
                                    <ul class="links collapse" ${ isNormalMode ? 'id="injectables-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' :
                                        'id="xs-injectables-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' }>
                                        <li class="link">
                                            <a href="injectables/HelperService.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >HelperService</a>
                                        </li>
                                        <li class="link">
                                            <a href="injectables/ModalService.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >ModalService</a>
                                        </li>
                                    </ul>
                                </li>
                                    <li class="chapter inner">
                                        <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ?
                                            'data-target="#pipes-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' : 'data-target="#xs-pipes-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' }>
                                            <span class="icon ion-md-add"></span>
                                            <span>Pipes</span>
                                            <span class="icon ion-ios-arrow-down"></span>
                                        </div>
                                        <ul class="links collapse" ${ isNormalMode ? 'id="pipes-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' :
                                            'id="xs-pipes-links-module-SharedModule-37f9fa27e7a391c8d5cddc6ac86e754d"' }>
                                            <li class="link">
                                                <a href="pipes/RgbToStringPipe.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >RgbToStringPipe</a>
                                            </li>
                                        </ul>
                                    </li>
                            </li>
                            <li class="link">
                                <a href="modules/SimulationDiagramModule.html" data-type="entity-link" >SimulationDiagramModule</a>
                                    <li class="chapter inner">
                                        <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ?
                                            'data-target="#components-links-module-SimulationDiagramModule-b57d4e06b6785bb444639d77bb5a0460"' : 'data-target="#xs-components-links-module-SimulationDiagramModule-b57d4e06b6785bb444639d77bb5a0460"' }>
                                            <span class="icon ion-md-cog"></span>
                                            <span>Components</span>
                                            <span class="icon ion-ios-arrow-down"></span>
                                        </div>
                                        <ul class="links collapse" ${ isNormalMode ? 'id="components-links-module-SimulationDiagramModule-b57d4e06b6785bb444639d77bb5a0460"' :
                                            'id="xs-components-links-module-SimulationDiagramModule-b57d4e06b6785bb444639d77bb5a0460"' }>
                                            <li class="link">
                                                <a href="components/DiagramComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >DiagramComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/DiagramLineComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >DiagramLineComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/LegendComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >LegendComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/LegendEntryComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >LegendEntryComponent</a>
                                            </li>
                                            <li class="link">
                                                <a href="components/SimulationDiagramComponent.html" data-type="entity-link" data-context="sub-entity" data-context-id="modules" >SimulationDiagramComponent</a>
                                            </li>
                                        </ul>
                                    </li>
                            </li>
                </ul>
                </li>
                    <li class="chapter">
                        <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ? 'data-target="#classes-links"' :
                            'data-target="#xs-classes-links"' }>
                            <span class="icon ion-ios-paper"></span>
                            <span>Classes</span>
                            <span class="icon ion-ios-arrow-down"></span>
                        </div>
                        <ul class="links collapse " ${ isNormalMode ? 'id="classes-links"' : 'id="xs-classes-links"' }>
                            <li class="link">
                                <a href="classes/Action.html" data-type="entity-link" >Action</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaEntity.html" data-type="entity-link" >asiaEntity</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaGroup.html" data-type="entity-link" >asiaGroup</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaGroupEntry.html" data-type="entity-link" >asiaGroupEntry</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaInfluencesRelation.html" data-type="entity-link" >asiaInfluencesRelation</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaInstance.html" data-type="entity-link" >asiaInstance</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaInstanceContainsRelation.html" data-type="entity-link" >asiaInstanceContainsRelation</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaInstanceProjectNetworkRelations.html" data-type="entity-link" >asiaInstanceProjectNetworkRelations</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaNetwork.html" data-type="entity-link" >asiaNetwork</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaNetworkStructure.html" data-type="entity-link" >asiaNetworkStructure</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaPerson.html" data-type="entity-link" >asiaPerson</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaProject.html" data-type="entity-link" >asiaProject</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaProjectContainsRelation.html" data-type="entity-link" >asiaProjectContainsRelation</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaRelation.html" data-type="entity-link" >asiaRelation</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaRelation-1.html" data-type="entity-link" >asiaRelation</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaSocialNode.html" data-type="entity-link" >asiaSocialNode</a>
                            </li>
                            <li class="link">
                                <a href="classes/asiaUser.html" data-type="entity-link" >asiaUser</a>
                            </li>
                            <li class="link">
                                <a href="classes/BasicCommand.html" data-type="entity-link" >BasicCommand</a>
                            </li>
                            <li class="link">
                                <a href="classes/DetailsNode.html" data-type="entity-link" >DetailsNode</a>
                            </li>
                            <li class="link">
                                <a href="classes/DetailsNodeHelper.html" data-type="entity-link" >DetailsNodeHelper</a>
                            </li>
                            <li class="link">
                                <a href="classes/ExampleMoveCommand.html" data-type="entity-link" >ExampleMoveCommand</a>
                            </li>
                            <li class="link">
                                <a href="classes/ExampleNode.html" data-type="entity-link" >ExampleNode</a>
                            </li>
                            <li class="link">
                                <a href="classes/InstanceDeleteAction.html" data-type="entity-link" >InstanceDeleteAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/InstanceInsertAction.html" data-type="entity-link" >InstanceInsertAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/InstanceUpdateAction.html" data-type="entity-link" >InstanceUpdateAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/NetworkDeleteAction.html" data-type="entity-link" >NetworkDeleteAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/NetworkInsertAction.html" data-type="entity-link" >NetworkInsertAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/NetworkStructureDeleteConnectionAction.html" data-type="entity-link" >NetworkStructureDeleteConnectionAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/NetworkStructureDeletePersonAction.html" data-type="entity-link" >NetworkStructureDeletePersonAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/NetworkStructureInsertConnectionAction.html" data-type="entity-link" >NetworkStructureInsertConnectionAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/NetworkStructureInsertGroupAction.html" data-type="entity-link" >NetworkStructureInsertGroupAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/NetworkStructureInsertPersonAction.html" data-type="entity-link" >NetworkStructureInsertPersonAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/NetworkStructureUpdateConnectionAction.html" data-type="entity-link" >NetworkStructureUpdateConnectionAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/NetworkStructureUpdatePersonAction.html" data-type="entity-link" >NetworkStructureUpdatePersonAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/NetworkUpdateAction.html" data-type="entity-link" >NetworkUpdateAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/ObservableCache.html" data-type="entity-link" >ObservableCache</a>
                            </li>
                            <li class="link">
                                <a href="classes/PaneContainerError.html" data-type="entity-link" >PaneContainerError</a>
                            </li>
                            <li class="link">
                                <a href="classes/PaneDividerError.html" data-type="entity-link" >PaneDividerError</a>
                            </li>
                            <li class="link">
                                <a href="classes/PaneError.html" data-type="entity-link" >PaneError</a>
                            </li>
                            <li class="link">
                                <a href="classes/ProjectDeleteAction.html" data-type="entity-link" >ProjectDeleteAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/ProjectHierarchyDeleteAction.html" data-type="entity-link" >ProjectHierarchyDeleteAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/ProjectHierarchyInsertAction.html" data-type="entity-link" >ProjectHierarchyInsertAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/ProjectHierarchyUpdateAction.html" data-type="entity-link" >ProjectHierarchyUpdateAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/ProjectInsertAction.html" data-type="entity-link" >ProjectInsertAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/ProjectUpdateAction.html" data-type="entity-link" >ProjectUpdateAction</a>
                            </li>
                            <li class="link">
                                <a href="classes/Shortcut.html" data-type="entity-link" >Shortcut</a>
                            </li>
                        </ul>
                    </li>
                        <li class="chapter">
                            <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ? 'data-target="#injectables-links"' :
                                'data-target="#xs-injectables-links"' }>
                                <span class="icon ion-md-arrow-round-down"></span>
                                <span>Injectables</span>
                                <span class="icon ion-ios-arrow-down"></span>
                            </div>
                            <ul class="links collapse " ${ isNormalMode ? 'id="injectables-links"' : 'id="xs-injectables-links"' }>
                                <li class="link">
                                    <a href="injectables/ActionsStateService.html" data-type="entity-link" >ActionsStateService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/AuthorizationService.html" data-type="entity-link" >AuthorizationService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/AuthorizationStoreService.html" data-type="entity-link" >AuthorizationStoreService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/CommandHistoryService.html" data-type="entity-link" >CommandHistoryService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/DataPanelNavigationService.html" data-type="entity-link" >DataPanelNavigationService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/GraphEditorSelectionService.html" data-type="entity-link" >GraphEditorSelectionService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/GroupService.html" data-type="entity-link" >GroupService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/HelperService.html" data-type="entity-link" >HelperService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/InputService.html" data-type="entity-link" >InputService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/InstanceProjectNetworkRelationsStoreService.html" data-type="entity-link" >InstanceProjectNetworkRelationsStoreService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/InstanceService.html" data-type="entity-link" >InstanceService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/InstanceStoreService.html" data-type="entity-link" >InstanceStoreService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/LoggerService.html" data-type="entity-link" >LoggerService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/MenubarService.html" data-type="entity-link" >MenubarService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/ModalService.html" data-type="entity-link" >ModalService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/NetworkService.html" data-type="entity-link" >NetworkService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/NetworkStoreService.html" data-type="entity-link" >NetworkStoreService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/NetworkStructureService.html" data-type="entity-link" >NetworkStructureService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/NetworkStructureStoreService.html" data-type="entity-link" >NetworkStructureStoreService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/PaneService.html" data-type="entity-link" >PaneService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/PersonService.html" data-type="entity-link" >PersonService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/ProjectHierarchyStoreService.html" data-type="entity-link" >ProjectHierarchyStoreService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/ProjectService.html" data-type="entity-link" >ProjectService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/ProjectStoreService.html" data-type="entity-link" >ProjectStoreService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/RelationService.html" data-type="entity-link" >RelationService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/UserService.html" data-type="entity-link" >UserService</a>
                                </li>
                            </ul>
                        </li>
                    <li class="chapter">
                        <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ? 'data-target="#interceptors-links"' :
                            'data-target="#xs-interceptors-links"' }>
                            <span class="icon ion-ios-swap"></span>
                            <span>Interceptors</span>
                            <span class="icon ion-ios-arrow-down"></span>
                        </div>
                        <ul class="links collapse " ${ isNormalMode ? 'id="interceptors-links"' : 'id="xs-interceptors-links"' }>
                            <li class="link">
                                <a href="interceptors/ErrorInterceptor.html" data-type="entity-link" >ErrorInterceptor</a>
                            </li>
                            <li class="link">
                                <a href="interceptors/JwtInterceptor.html" data-type="entity-link" >JwtInterceptor</a>
                            </li>
                        </ul>
                    </li>
                    <li class="chapter">
                        <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ? 'data-target="#interfaces-links"' :
                            'data-target="#xs-interfaces-links"' }>
                            <span class="icon ion-md-information-circle-outline"></span>
                            <span>Interfaces</span>
                            <span class="icon ion-ios-arrow-down"></span>
                        </div>
                        <ul class="links collapse " ${ isNormalMode ? ' id="interfaces-links"' : 'id="xs-interfaces-links"' }>
                            <li class="link">
                                <a href="interfaces/HSV.html" data-type="entity-link" >HSV</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/IBaseNode.html" data-type="entity-link" >IBaseNode</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/ICommand.html" data-type="entity-link" >ICommand</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/IPosition.html" data-type="entity-link" >IPosition</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/ISelectable.html" data-type="entity-link" >ISelectable</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/IStoreService.html" data-type="entity-link" >IStoreService</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/IStoreServiceOptimistic.html" data-type="entity-link" >IStoreServiceOptimistic</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/IStoreServicePessimistic.html" data-type="entity-link" >IStoreServicePessimistic</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/RGB.html" data-type="entity-link" >RGB</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/RGB-1.html" data-type="entity-link" >RGB</a>
                            </li>
                        </ul>
                    </li>
                    <li class="chapter">
                        <div class="simple menu-toggler" data-toggle="collapse" ${ isNormalMode ? 'data-target="#miscellaneous-links"'
                            : 'data-target="#xs-miscellaneous-links"' }>
                            <span class="icon ion-ios-cube"></span>
                            <span>Miscellaneous</span>
                            <span class="icon ion-ios-arrow-down"></span>
                        </div>
                        <ul class="links collapse " ${ isNormalMode ? 'id="miscellaneous-links"' : 'id="xs-miscellaneous-links"' }>
                            <li class="link">
                                <a href="miscellaneous/enumerations.html" data-type="entity-link">Enums</a>
                            </li>
                            <li class="link">
                                <a href="miscellaneous/functions.html" data-type="entity-link">Functions</a>
                            </li>
                            <li class="link">
                                <a href="miscellaneous/typealiases.html" data-type="entity-link">Type aliases</a>
                            </li>
                            <li class="link">
                                <a href="miscellaneous/variables.html" data-type="entity-link">Variables</a>
                            </li>
                        </ul>
                    </li>
                        <li class="chapter">
                            <a data-type="chapter-link" href="routes.html"><span class="icon ion-ios-git-branch"></span>Routes</a>
                        </li>
                    <li class="chapter">
                        <a data-type="chapter-link" href="coverage.html"><span class="icon ion-ios-stats"></span>Documentation coverage</a>
                    </li>
                    <li class="divider"></li>
                    <li class="copyright">
                        Documentation generated using <a href="https://compodoc.app/" target="_blank">
                            <img data-src="images/compodoc-vectorise.png" class="img-responsive" data-type="compodoc-logo">
                        </a>
                    </li>
            </ul>
        </nav>
        `);
        this.innerHTML = tp.strings;
    }
});